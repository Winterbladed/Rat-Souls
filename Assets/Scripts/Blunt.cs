using UnityEngine;

//Blunt
//Deals Damage
public class Blunt : Status
{
    #region Private Functions
    protected override void Start()
    {
        base.Start();
        _statusName = "Blunt";
        _statusColor = Color.white;
    }

    protected void Update()
    {
        if (_isActive)
        {
            _statusTime += Time.deltaTime;
            _statusTick += Time.deltaTime;
            if (_statusTick > _statusTicker)
            {
                _statusTick = 0.0f;
            }
            if (_statusTime > _statusTimer)
            {
                _statusTime = 0.0f;
                DisableStatus();
            }
        }
    }
    #endregion
}