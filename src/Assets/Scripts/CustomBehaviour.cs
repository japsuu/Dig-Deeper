using UnityEngine;

/// <summary>
/// <see cref="MonoBehaviour"/> with a custom update mode.
/// </summary>
public abstract class CustomBehaviour : MonoBehaviour
{
    [SerializeField]
    private ScriptUpdateMode _scriptUpdateMode = ScriptUpdateMode.Update;
    
    
    public void SetUpdateMode(ScriptUpdateMode updateMode)
    {
        _scriptUpdateMode = updateMode;
    }


    protected virtual void Update()
    {
        if (_scriptUpdateMode == ScriptUpdateMode.Update)
            InternalUpdate();
    }


    protected virtual void FixedUpdate()
    {
        if (_scriptUpdateMode == ScriptUpdateMode.FixedUpdate)
            InternalUpdate();
    }
        
        
    public void ManualUpdate()
    {
        if (_scriptUpdateMode == ScriptUpdateMode.Manual)
            InternalUpdate();
    }


    protected abstract void InternalUpdate();
}