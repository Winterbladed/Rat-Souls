using UnityEngine;
[RequireComponent (typeof(TextEvent))]

public class Damage : MonoBehaviour
{
    #region Variables
    public enum DamageType
    {
        _None,
        //Base Physical
        _Blunt,
        _Pierce,
        _Slash,
        //Base Elemental
        _Toxin,
        _Ice,
        _Fire,
        _Electric,
        //Advanced Elemental
        _Virus,
        _Gas,
        _Corrode,
        _Melt,
        _Magnetic,
        _Blast,
    }

    [Header("Damage Stats")]
    public DamageType _DamageType;
    public int _Damage;
    protected int _normalDamage;
    protected int _reducedDamage;
    protected int _computedDamage = 0;

    [Header("Critical Stats")]
    public int _CriticalDamage;
    public float _CriticalChance;
    protected float _currentCriticalChance = 0.0f;
    protected bool _isCritical = false;

    [Header("Status Stats")]
    public float _StatusChance;
    protected float _currentStatusChance = 0.0f;
    protected bool _isStatus = false;

    [Header("Status Effect Stats")]
    public int _StatusDamage = 0;
    public float _StatusTimer = 0.0f;
    public float _StatusTicker = 0.0f;

    protected TextEvent _textEvent;
    #endregion

    #region Private Functions
    protected virtual void Start()
    {
        _textEvent = GetComponent<TextEvent>();
        _normalDamage = _Damage; _reducedDamage = _Damage / 2;
    }
    #endregion

    #region Protected Functions
    protected void CriticalDamageChance()
    {
        _Damage = _normalDamage;
        _currentCriticalChance = Random.Range(0.0f, 1.0f);
        if (_currentCriticalChance <= _CriticalChance)
        {
            _Damage *= _CriticalDamage;
            _isCritical = true;
        }
        else _isCritical = false;
    }

    protected void StatusChance()
    {
        _currentStatusChance = Random.Range(0.0f, 1.0f);
        if (_currentStatusChance <= _StatusChance)
        {
            _isStatus = true;
        }
        else _isStatus = false;
    }

    protected void DamageEvent(int _number, GameObject _target)
    {
        _computedDamage = _number;
        TextDamage(_target);
    }

    protected void TextDamage(GameObject _target)
    {
        Health _health = _target.GetComponent<Health>();
        if (_computedDamage * _health.GetHpDamageMultiplier() <= 0)
        {
            _computedDamage = 0;
            _textEvent.ShowDamage(0, Color.gray, _target.transform);
        }
        if (!_isCritical && _computedDamage * _health.GetHpDamageMultiplier() > 0)
        {
            if (!_health.GetIsBlocking()) _textEvent.ShowDamage(_computedDamage * _health.GetHpDamageMultiplier(), Color.white, _target.transform);
            else if (_health.GetIsBlocking())
            {
                _textEvent.ShowState("Blocked!", Color.white, _target.transform);
                _textEvent.ShowDamage((_computedDamage * _health.GetHpDamageMultiplier()) / 4, Color.white, _target.transform);
            }
        }
        if (_isCritical && _computedDamage * _health.GetHpDamageMultiplier() > 0)
        {
            if (!_health.GetIsBlocking()) _textEvent.ShowDamage(_computedDamage * _health.GetHpDamageMultiplier(), Color.yellow, _target.transform);
            else if (_health.GetIsBlocking())
            {
                _textEvent.ShowState("Blocked!", Color.white, _target.transform);
                _textEvent.ShowDamage((_computedDamage * _health.GetHpDamageMultiplier()) / 4, Color.yellow, _target.transform);
            }
        }
    }

    protected void DealDamage(GameObject _target)
    {
        //Target has Health
        if (_target.gameObject.GetComponent<Health>())
        {
            CriticalDamageChance();
            StatusChance();
            //Target has Armor and Shield and Health
            if (_target.gameObject.GetComponent<Armor>() && _target.gameObject.GetComponent<Shield>())
            {
                Health _health = _target.gameObject.GetComponent<Health>();
                Armor _armor = _target.gameObject.GetComponent<Armor>();
                Shield _shield = _target.gameObject.GetComponent<Shield>();
                if (_health.GetCurrentHp() > 0 && !_health.GetIsInvulnerable())
                {
                    if (_shield.GetCurrentSp() <= 0)
                    {
                        int _damage = _Damage - _armor.GetCurrentAp();
                        DamageEvent(_damage, _target.gameObject);
                        _health.TakeHpDamage(_computedDamage);
                    }
                    else
                    {
                        DamageEvent(_Damage, _target.gameObject);
                        _shield.TakeSpDamage(_computedDamage);
                    }
                    if (_isStatus && _target.gameObject.GetComponent<Status>()) DealStatusEffect(_target.gameObject);
                }
            }
            //Target has Shield and Health only
            else if (!_target.gameObject.GetComponent<Armor>() && _target.gameObject.GetComponent<Shield>())
            {
                Health _health = _target.gameObject.GetComponent<Health>();
                Shield _shield = _target.gameObject.GetComponent<Shield>();
                if (_health.GetCurrentHp() > 0 && !_health.GetIsInvulnerable())
                {
                    DamageEvent(_Damage, _target.gameObject);
                    if (_shield.GetCurrentSp() <= 0) _health.TakeHpDamage(_computedDamage);
                    else _shield.TakeSpDamage(_computedDamage);
                    if (_isStatus && _target.gameObject.GetComponent<Status>()) DealStatusEffect(_target.gameObject);
                }
            }
            //Target has Armor and Health only
            else if (_target.gameObject.GetComponent<Armor>() && !_target.gameObject.GetComponent<Shield>())
            {
                Health _health = _target.gameObject.GetComponent<Health>();
                Armor _armor = _target.gameObject.GetComponent<Armor>();
                if (_health.GetCurrentHp() > 0 && !_health.GetIsInvulnerable())
                {
                    int _damage = _Damage - _armor.GetCurrentAp();
                    DamageEvent(_damage, _target.gameObject);
                    _health.TakeHpDamage(_computedDamage);
                    if (_isStatus && _target.gameObject.GetComponent<Status>()) DealStatusEffect(_target.gameObject);
                }
            }
            //Target has Health only
            else
            {
                Health _health = _target.gameObject.GetComponent<Health>();
                if (_health.GetCurrentHp() > 0 && !_health.GetIsInvulnerable())
                {
                    DamageEvent(_Damage, _target.gameObject);
                    _health.TakeHpDamage(_computedDamage);
                    if (_isStatus && _target.gameObject.GetComponent<Status>()) DealStatusEffect(_target.gameObject);
                }
            }
        }
    }

    protected void DealStatusEffect(GameObject _target)
    {
        //Base Physical Status Effects
        Blunt _blunt = _target.GetComponent<Blunt>();
        Pierce _pierce = _target.GetComponent<Pierce>();
        Slash _slash = _target.GetComponent<Slash>();

        //Base Elemental Status Effects
        Toxin _toxin = _target.GetComponent<Toxin>();
        Ice _ice = _target.GetComponent<Ice>();
        Fire _fire = _target.GetComponent<Fire>();
        Electric _electric = _target.GetComponent<Electric>();

        //Advanced Elemental Status Effects
        Virus _virus = _target.GetComponent<Virus>();
        Gas _gas = _target.GetComponent<Gas>();
        Corrode _corrode = _target.GetComponent<Corrode>();
        Melt _melt = _target.GetComponent<Melt>();
        Magnetic _magnetic = _target.GetComponent<Magnetic>();
        Blast _blast = _target.GetComponent<Blast>();

        //Blunt Status Effect : 
        if (_blunt && _DamageType == DamageType._Blunt && !_blunt.GetIsActive())
        {
            SetStatusStats(_blunt, _StatusDamage, _StatusTimer, _StatusTicker); //Modify Status Stats
            _blunt.EnableStatus(); //Trigger Physical Status
            _textEvent.ShowStatus(_blunt.GetStatusName(), _blunt.GetStatusColor(), _blunt.GetStatusSprite(), _target.transform);
        }

        //Pierce Status Effect : 
        else if(_pierce && _DamageType == DamageType._Pierce && !_pierce.GetIsActive())
        {
            SetStatusStats(_pierce, _StatusDamage, _StatusTimer, _StatusTicker); //Modify Status Stats
            _pierce.EnableStatus(); //Trigger Physical Status
        }

        //Slash Status Effect : Deals Damage overtime that bypasses Armor during the effect
        else if (_slash && _DamageType == DamageType._Slash && !_slash.GetIsActive())
        {
            SetStatusStats(_slash, _StatusDamage, _StatusTimer, _StatusTicker); //Modify Status Stats
            _slash.EnableStatus(); //Trigger Physical Status
        }

        //Toxin Status Effect : Deals Damage overtime that bypasses Shields during the effect
        else if (_toxin && _DamageType == DamageType._Toxin && !_toxin.GetIsActive() && !_ice.GetIsActive() && !_fire.GetIsActive() && !_electric.GetIsActive())
        {
            SetStatusStats(_toxin, _StatusDamage, _StatusTimer, _StatusTicker); //Modify Status Stats
            _toxin.EnableStatus(); //Trigger Elemental Status
        }

        //Ice Status Effect : Slows down Movement Speed during the effect
        else if (_ice && _DamageType == DamageType._Ice && !_toxin.GetIsActive() && !_ice.GetIsActive() && !_fire.GetIsActive() && !_electric.GetIsActive())
        {
            SetStatusStats(_ice, _StatusDamage, _StatusTimer, _StatusTicker); //Modify Status Stats
            _ice.EnableStatus(); //Trigger Elemental Status
        }

        //Fire Status Effect : Deals Damage overtime during the effect
        else if (_fire && _DamageType == DamageType._Fire && !_toxin.GetIsActive() && !_ice.GetIsActive() && !_fire.GetIsActive() && !_electric.GetIsActive())
        {
            SetStatusStats(_fire, _StatusDamage, _StatusTimer, _StatusTicker); //Modify Status Stats
            _fire.EnableStatus(); //Trigger Elemental Status
        }

        //Electric Status Effect : Deals Damage overtime during the effect
        else if (_electric && _DamageType == DamageType._Electric && !_toxin.GetIsActive() && !_ice.GetIsActive() && !_fire.GetIsActive() && !_electric.GetIsActive())
        {
            SetStatusStats(_electric, _StatusDamage, _StatusTimer, _StatusTicker); //Modify Status Stats
            _electric.EnableStatus(); //Trigger Elemental Status
        }

        //Virus Status Effect = Toxin + Ice : Modifies Health to take more damage from all sources during the effect
        else if (_virus && _DamageType == DamageType._Virus && !_virus.GetIsActive() ||
            _virus && _DamageType == DamageType._Toxin && !_virus.GetIsActive() && _ice.GetIsActive() ||
            _virus && _DamageType == DamageType._Ice && !_virus.GetIsActive() && _toxin.GetIsActive())
        {
            SetStatusStats(_virus, _StatusDamage, _StatusTimer * 2.0f, _StatusTicker); //Modify Status Stats
            _virus.EnableStatus(); //Trigger Elemental Fusion
            _toxin.DisableStatus(); _ice.DisableStatus(); //Disable Base Element Status on Elemental Fusion
        }

        //Gas Status Effect = Toxin + Fire : Deals Damage overtime in an Area Of Effect and disables Health Regen during the effect
        else if (_gas && _DamageType == DamageType._Gas && !_gas.GetIsActive() ||
            _gas && _DamageType == DamageType._Toxin && !_gas.GetIsActive() && _fire.GetIsActive() ||
            _gas && _DamageType == DamageType._Fire && !_gas.GetIsActive() && _toxin.GetIsActive())
        {
            SetStatusStats(_gas, _StatusDamage, _StatusTimer, _StatusTicker); //Modify Status Stats
            _gas.EnableStatus(); //Trigger Elemental Fusion
            _toxin.DisableStatus(); _fire.DisableStatus(); //Disable Base Element Status on Elemental Fusion
        }

        //Corrode Status Effect = Toxin + Electric : Disables Armor during the effect
        else if (_corrode && _DamageType == DamageType._Corrode && !_corrode.GetIsActive() ||
            _corrode && _DamageType == DamageType._Toxin && !_corrode.GetIsActive() && _electric.GetIsActive() ||
            _corrode && _DamageType == DamageType._Electric && !_corrode.GetIsActive() && _toxin.GetIsActive())
        {
            SetStatusStats(_corrode, _StatusDamage, _StatusTimer * 2.0f, _StatusTicker); //Modify Status Stats
            _corrode.EnableStatus(); //Trigger Elemental Fusion
            _toxin.DisableStatus(); _electric.DisableStatus(); //Disable Base Element Status on Elemental Fusion
        }

        //Melt Status Effect = Ice + Fire : Weakens Damage sources during the effect
        else if (_melt && _DamageType == DamageType._Melt && !_melt.GetIsActive() ||
            _melt && _DamageType == DamageType._Ice && !_melt.GetIsActive() && _fire.GetIsActive() ||
            _melt && _DamageType == DamageType._Fire && !_melt.GetIsActive() && _ice.GetIsActive())
        {
            SetStatusStats(_melt, _StatusDamage, _StatusTimer * 2.0f, _StatusTicker); //Modify Status Stats
            _melt.EnableStatus(); //Trigger Elemental Fusion
            _ice.DisableStatus(); _fire.DisableStatus(); //Disable Base Element Status on Elemental Fusion
        }

        //Magnetic Status Effect = Ice + Electric : Disables Shield during the effect
        else if (_magnetic && _DamageType == DamageType._Magnetic && !_magnetic.GetIsActive() ||
            _magnetic && _DamageType == DamageType._Ice && !_magnetic.GetIsActive() && _electric.GetIsActive() ||
            _magnetic && _DamageType == DamageType._Electric && !_magnetic.GetIsActive() && _ice.GetIsActive())
        {
            SetStatusStats(_magnetic, _StatusDamage, _StatusTimer * 2.0f, _StatusTicker); //Modify Status Stats
            _magnetic.EnableStatus(); //Trigger Elemental Fusion
            _ice.DisableStatus(); _electric.DisableStatus(); //Disable Base Element Status on Elemental Fusion
        }

        //Blast Status Effect = Fire + Electric : Deals instant Area Of Effect Damage on effect
        else if (_blast && _DamageType == DamageType._Blast && !_blast.GetIsActive() ||
            _blast && _DamageType == DamageType._Fire && !_blast.GetIsActive() && _electric.GetIsActive() ||
            _blast && _DamageType == DamageType._Electric && !_blast.GetIsActive() && _fire.GetIsActive())
        {
            SetStatusStats(_blast, _StatusDamage, _StatusTimer, _StatusTicker); //Modify Status Stats
            _blast.EnableStatus(); //Trigger Elemental Fusion
            _fire.DisableStatus(); _electric.DisableStatus(); //Disable Base Element Status on Elemental Fusion
        }
    }

    protected void SetStatusStats(Status _statusEffect, int _damage, float _time, float _tick)
    {
        _statusEffect.SetStatusDamage(_damage);
        _statusEffect.SetStatusTimer(_time);
        _statusEffect.SetStatusTicker(_tick);
    }
    #endregion

    #region Public Functions
    public void CrippleDamage() { _Damage = _reducedDamage; }
    public void UnCrippleDamage() { _Damage = _normalDamage; }
    #endregion
}