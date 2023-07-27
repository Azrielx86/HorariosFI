namespace HorariosFI.Core.Exeptions;

internal class RobotDetectedException : Exception
{
    public override string Message => "Automatización detectada! Ingresa a la página MisProfesores e ingresa el captcha";

    public override string ToString() => $"RobotDetectedException: {Message}";
}