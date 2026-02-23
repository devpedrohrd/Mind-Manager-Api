public enum UserRole { Admin, Client, Psychologist }
public enum Gender { Male, Female, Other }
public enum CreatedBy { Patient, Psychologist }
public enum PatientType { Student, Contractor, Guardian, Teacher }
public enum Education { Medio, Superior, PosGraduacao, Tecnico, Mestrado }
public enum Courses { Fisica, Quimica, Ads, Eletrotecnica, Administracao, Informatica }
public enum Status { Pending, Confirmed, Finalized, Canceled, Absence }
public enum TypeAppointment { Session, CollectiveActivities, AdministrativeRecords }
public enum ActivityType { Group, Lecture, Seminar, Meeting, DiscussionCircle }

public enum PsychologicalDisorder
{
    Depression, GeneralizedAnxiety, BipolarDisorder, Borderline,
    Schizophrenia, OCD, PTSD, ADHD, Autism, EatingDisorder,
    SubstanceAbuse, PersonalityDisorder, PanicDisorder, Psychosis, Other
}

public enum Difficulty
{
    Avaliation, OrganizationOnStudies, Concentration,
    Memory, Tdah, Comunication, Relationship, Other
}