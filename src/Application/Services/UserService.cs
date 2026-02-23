using Mind_Manager.Domain.Entities;
using Mind_Manager.Domain.Exceptions;
using Mind_Manager.Domain.Interfaces;
using Mind_Manager.Domain.Validators;
using Mind_Manager.Application.Mappers;
using Mind_Manager.Application.Authorization;
using Mind_Manager.src.Domain.DTO;

namespace Mind_Manager;

public interface IUserService
{
    Task<UserResponse> GetByIdAsync(Guid id, Guid? requestingUserId, string? requestingUserRole);
    Task<UserResponse> GetByEmailAsync(string email);
    Task<UserResponse> CreateAsync(CreateUserCommand createUserDto);
    Task<UserResponse> UpdateAsync(Guid id, UpdateUserCommand updateUserDto, Guid? requestingUserId, string? requestingUserRole);
    Task<bool> DeleteAsync(Guid id, Guid? requestingUserId, string? requestingUserRole);
    Task<SearchUsersResponse> SearchAsync(SearchUsersQuery searchDto, Guid? userId = null, string? userRole = null);
}

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserValidator _validator;
    private readonly IAuthorizationService _authorizationService;

    public UserService(
        IUnitOfWork unitOfWork,
        IUserValidator validator,
        IAuthorizationService authorizationService)
    {
        _unitOfWork = unitOfWork;
        _validator = validator;
        _authorizationService = authorizationService;
    }


    public async Task<UserResponse> CreateAsync(CreateUserCommand createUserDto)
    {
        _validator.ValidateEmail(createUserDto.Email);
        _validator.ValidatePhone(createUserDto.Phone);
        _validator.ValidatePassword(createUserDto.Password);

        var userExists = await _unitOfWork.Users.EmailExistsAsync(createUserDto.Email);
        if (userExists)
            throw new BusinessException("EMAIL_ALREADY_EXISTS");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password, 12);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = createUserDto.Name,
            Email = createUserDto.Email,
            PasswordHash = passwordHash,
            Role = UserRole.Client,
            IsActive = true,
            Phone = createUserDto.Phone,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return UserMapper.ToResponseDto(user);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid? requestingUserId, string? requestingUserRole)
    {   
        // Verificação de autorização obrigatória
        if (requestingUserId.HasValue && !string.IsNullOrEmpty(requestingUserRole))
        {
            if (Enum.TryParse<UserRole>(requestingUserRole, out var role))
            {
                _authorizationService.ValidateResourceOwnership(id, requestingUserId.Value, role);
            }
            else
            {
                throw new ForbiddenException("Invalid user role");
            }
        }
        else
        {
            throw new ForbiddenException("Authentication required");
        }
        
        var deleted = await _unitOfWork.Users.DeleteAsync(id);
        if (deleted)
            await _unitOfWork.SaveChangesAsync();

        return deleted;
    }

    public async Task<UserResponse> GetByEmailAsync(string email)
    {
        _validator.ValidateEmail(email);
        var user = await _unitOfWork.Users.GetByEmailAsync(email);

        if (user == null)
            throw new NotFoundException("USER_NOT_FOUND");

        return UserMapper.ToResponseDto(user);
    }

    public async Task<UserResponse> GetByIdAsync(Guid id, Guid? requestingUserId, string? requestingUserRole)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);

        if (user == null)
            throw new NotFoundException("USER_NOT_FOUND");

        // Verificação de autorização
        if (requestingUserId.HasValue && !string.IsNullOrEmpty(requestingUserRole))
        {
            if (Enum.TryParse<UserRole>(requestingUserRole, out var role))
            {
                _authorizationService.ValidateResourceOwnership(id, requestingUserId.Value, role);
            }
            else
            {
                throw new ForbiddenException("Invalid user role");
            }
        }
        else
        {
            throw new ForbiddenException("Authentication required");
        }

        return UserMapper.ToResponseDto(user);
    }

    public async Task<UserResponse> UpdateAsync(Guid id, UpdateUserCommand updateUserDto, Guid? requestingUserId, string? requestingUserRole)
    {
        _validator.ValidateEmail(updateUserDto.Email);
        _validator.ValidatePhone(updateUserDto.Phone);
        
        // Verificação de autorização obrigatória
        if (requestingUserId.HasValue && !string.IsNullOrEmpty(requestingUserRole))
        {
            if (Enum.TryParse<UserRole>(requestingUserRole, out var role))
            {
                _authorizationService.ValidateResourceOwnership(id, requestingUserId.Value, role);
            }
            else
            {
                throw new ForbiddenException("Invalid user role");
            }
        }
        else
        {
            throw new ForbiddenException("Authentication required");
        }
        
        var user = await _unitOfWork.Users.GetByIdAsync(id);

        if (user == null)
            throw new NotFoundException("USER_NOT_FOUND");

        user.UpdateProfile(updateUserDto.Name, updateUserDto.Email, updateUserDto.Phone);

        // Apenas administradores podem alterar role e status IsActive
        if (Enum.Parse<UserRole>(requestingUserRole) == UserRole.Admin)
        {
            if (updateUserDto.Role.HasValue && Enum.TryParse<UserRole>(updateUserDto.Role.Value.ToString(), out var role))
                user.ChangeRole(role);

            if (updateUserDto.IsActive.HasValue)
            {
                if (updateUserDto.IsActive.Value) user.Activate();
                else user.Deactivate();
            }
        }
        // Usuários comuns não podem alterar role nem status
        else if (updateUserDto.Role.HasValue || updateUserDto.IsActive.HasValue)
        {
            throw new ForbiddenException("Only administrators can change user role or activation status");
        }

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return UserMapper.ToResponseDto(user);
    }
    public async Task<SearchUsersResponse> SearchAsync(SearchUsersQuery searchDto, Guid? userId = null, string? userRole = null)
    {
        // Garantir que Filters não seja null
        var filters = searchDto.Filters ?? new UserFilters();
        
        if (userId.HasValue && !string.IsNullOrEmpty(userRole))
        {
            if (userRole == UserRole.Psychologist.ToString() || userRole == UserRole.Client.ToString())
            {
                filters = filters with { Id = userId.Value };
            }
        }

        var finalSearchDto = searchDto with { Filters = filters };
        var pagedUsers = await _unitOfWork.Users.SearchUsersAsync(finalSearchDto);
        
        var totalPages = (int)Math.Ceiling((double)pagedUsers.Total / pagedUsers.Limit);
        var userResponses = pagedUsers.Data.ToList().AsReadOnly();
        
        return new SearchUsersResponse(
            Data: userResponses,
            Total: pagedUsers.Total,
            Page: pagedUsers.Page,
            Limit: pagedUsers.Limit,
            TotalPages: totalPages
        );
    }
}
