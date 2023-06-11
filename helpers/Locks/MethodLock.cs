using System;
using System.Reflection;

using helpers;

public class MethodLock
{
    private readonly MethodBase _lockedMethod;
    private bool _isLocked;

    public bool IsLocked => _isLocked;
    
    public MethodLock(int skipFrames = 0) => _lockedMethod = Reflection.GetExecutingMethod(skipFrames);

    public void SetLock(bool status, int skipFrames = 0)
    {
        if (!CanManage(true, skipFrames))  
            throw new UnauthorizedAccessException($"This lock can be managed only from {_lockedMethod.Name} (type {_lockedMethod.DeclaringType.FullName})");
        else 
            _isLocked = status;
    }
    
    public void ThrowIfUnauthorized(int skipFrames = 0) 
    { 
        if (!CanManage(true, skipFrames)) 
            throw new UnauthorizedAccessException($"This lock can be managed only from {_lockedMethod.Name} (type {_lockedMethod.DeclaringType.FullName})"); 
    }

    public bool CanManage(bool shouldSkip = true, int skipFrames = 0) => !_isLocked || Reflection.GetExecutingMethod(shouldSkip ? 1  + skipFrames : skipFrames) == _lockedMethod;
}