namespace NetDaemon.HassModel.Entities;

/// <summary>
/// 
/// </summary>
public interface ILightServiceTarget: IServiceTarget, IOnOffTarget
{
}

/// <summary>
/// 
/// </summary>
public interface ILightEntity : ILightServiceTarget
{
}