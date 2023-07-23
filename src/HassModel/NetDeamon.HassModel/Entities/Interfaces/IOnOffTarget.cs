namespace NetDaemon.HassModel.Entities;


/// <summary>
/// Base interface for togglebal targets
/// </summary>
public interface IOnOffTarget: IEntityTarget
{   
    // Virtual extension methods (aka default interface methods)

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void Toggle()
    {
        CallService("toggle");
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void TurnOn()
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void TurnOff()
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="targets"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void Toggle(IEnumerable<IEntityTarget> targets)
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="targets"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void TurnOn(IEnumerable<IEntityTarget> targets)
    {
        throw new NotImplementedException();
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="targets"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void TurnOff(IEnumerable<IEntityTarget> targets)
    {
        throw new NotImplementedException();
    }
}
