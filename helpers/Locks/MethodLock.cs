using System;
using System.Reflection;

using helpers;

public class MethodLock
{
    private readonly MethodBase _lockedMethod;
    private bool _isLocked;

    public bool IsLocked => _isLocked;
    
    public MethodLock() => _lockedMethod = Reflection.GetExecutingMethod();

    public void SetLock(bool status)
    {
        if (!CanManage())  throw new UnauthorizedAccessException($"This lock can be managed only from {_lockedMethod.Name} (type {_lockedMethod.DeclaringType.FullName})");
        else _isLocked = status;
    }
    
    public void ThrowIfUnauthorized() { if (!CanManage()) throw new UnauthorizedAccessException($"This lock can be managed only from {_lockedMethod.Name} (type {_lockedMethod.DeclaringType.FullName})"); }
    public bool CanManage(bool shouldSkip = true) => !_isLocked || Reflection.GetExecutingMethod(shouldSkip ? 1 : 0) == _lockedMethod;
}