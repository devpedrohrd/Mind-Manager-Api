# Mind Manager - Configuração

## Configuração dos arquivos de configuração

1. Copie `appsettings.template.json` para `appsettings.json`
2. Copie `appsettings.template.json` para `appsettings.Development.json`
3. Edite os arquivos criados com suas configurações reais:

### Connection Strings
- Configure sua string de conexão do banco PostgreSQL
- Para desenvolvimento, use um banco local ou de teste

### JWT Settings
- Gere uma chave secreta forte (mínimo 32 caracteres)
- Mantenha diferentes chaves para desenvolvimento e produção

### SMTP Settings
- Configure seu provedor de email
- Use senhas de aplicativo, não a senha principal da conta

### Variáveis de Ambiente (Recomendado para produção)
```bash
CONNECTIONSTRINGS__DEFAULTCONNECTION="sua_connection_string"
JWT__KEY="sua_chave_jwt_secreta"
SMTP__USERNAME="seu_email"
SMTP__PASSWORD="sua_senha_app"
```

**⚠️ IMPORTANTE:** 
- Nunca commite arquivos com dados sensíveis
- Use variáveis de ambiente em produção
- Mantenha backups seguros das configurações