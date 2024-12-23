using UnityEngine;

//Magnetic = Ice + Electric
//Disables Shield during the effect
public class Magnetic : Status
{
    #region Variables
    protected Shield _shield;
    #endregion

    #region Private Functions
    protected override void Start()
    {
        base.Start();
        _statusName = "Magnetic";
        _statusColor = Color.blue;
        _shield = GetComponent<Shield>();
    }

    protected void Update()
    {
        if (_isActive)
        {
            _statusTime += Time.deltaTime;
            _shield.ShieldDisable(true);
            if (_statusTime > _statusTimer)
            {
                _shield.ShieldDisable(false);
                DisableStatus();
            }
        }
    }
    #endregion
}