namespace NetDaemon.HassModel.Entities;

public interface ILightServiceTarget: IServiceTarget, IOnOffTarget //, IOnOffListTarget
{
}

public interface ILightEntity : ILightServiceTarget
{
}