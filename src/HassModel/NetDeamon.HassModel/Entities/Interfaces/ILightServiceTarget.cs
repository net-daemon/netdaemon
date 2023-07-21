namespace NetDaemon.HassModel.Entities;

public interface ILightServiceTarget: IServiceTarget, IOnOffTarget
{
}

public interface ILightEntity : ILightServiceTarget
{
}