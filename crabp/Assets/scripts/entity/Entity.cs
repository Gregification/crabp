using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public enum _DamageType{
    thermal,
    em,
    kinetic,
    mental
}

public class Entity : MonoBehaviour
{
    [Header("Entity")]
    public EntityValues eValues_base, eValues_effective;
    public Buffmanager BuffManager;
    private DateTime Buff_earliestTermination = DateTime.MaxValue;

    public void addBuff(Buff b)
    {
        if(b.isTemporary && (b.termEndDate == DateTime.MaxValue || b.termEndDate != null))//saw somewhere that datetime couldnt be nulled but it dosent give warnings for cheking if its null so eh, put it last so compiler dosent bother it it really does
        {
            if (DateTime.Compare(DateTime.Now, b.termEndDate) > 0)//if the time has yet to come
            {
                BuffManager.addBuff(b);
                if(DateTime.Compare(Buff_earliestTermination, b.termEndDate) > 0)
                {
                    StopCoroutine(_Buffs_temporary_timer());
                    StartCoroutine(_Buffs_temporary_timer());
                }
            }
        }
        else
        { 
            b.isTemporary = false;
            b.termEndDate = DateTime.Now;//records when it was added. tis does not reset on load as the buffs arnt removed
            BuffManager.addBuff(b);
        }

    }
    public bool removeBuff(Buff b)
    {
        return false;
    }
    IEnumerator _Buffs_temporary_timer()
    {
        int numRetainPerSearch = 4;
        Queue<Buff> daLine = new Queue<Buff>(numRetainPerSearch+1);
        bool moreBuffs = false;

        do
        {
            daLine.Clear();

            foreach (var v in BuffManager._buffs.Where(b => b.isTemporary).OrderByDescending(c => c.termEndDate.Date).ThenBy(c => c.termEndDate.TimeOfDay).Take(numRetainPerSearch))
                daLine.Enqueue(v);

            if (daLine.Count() == 0)
                yield return null;

            moreBuffs = daLine.Count == numRetainPerSearch;

            while (daLine.Count() > 0)//!!!danger!!! D: O:>
            {
                yield return new WaitForSeconds((float)(daLine.Peek().termEndDate - DateTime.Now).TotalSeconds);
                BuffManager.removeBuff(daLine.Dequeue());
            }

        } while (moreBuffs);
    }
}

public class EntityValues
{
    [Header("Entity Values")]
    public float health = 0, health_max = 0;
    public float health_regen = 0;
    public float stamina = 0, stamina_max = 0;
    public float stamina_regen = 0;
    public float speedScaler = 1;
    public Damages resistances;

    public EntityValues()
    {
        this.resistances = new Damages(0, 0, 0, 0);
    }
    public EntityValues(Damages resistances)
    {
        this.resistances = resistances;
    }
    public float[]  getVars()
    {
        return new float[]
        {
            //eV
            health, health_max,
            health_regen,
            stamina, stamina_max,
            stamina_regen,
            speedScaler,

            //resistances
            resistances.values[0],
            resistances.values[1],
            resistances.values[2],
            resistances.values[3]
        };

        /*var bindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        return GetType().GetFields(bindingFlags)
            //.Where(field => field.GetType().Equals(typeof(int)) || field.GetType().Equals(typeof(float)) || field.GetType().Equals(typeof(double)))
            .Select(field => field.GetValue(this))
            .Cast<float>().ToArray();//assuming all vars are numbers snabs all fieldInfos and casts to float as array*/
    }
    public void setVars(float[] n)
    {
        health = n[0]; health_max = n[1];
        health_regen = n[2];
        stamina = n[3]; stamina_max = n[4];
        stamina_regen = n[5];
        speedScaler = n[6];

        if (n.Length < 6) return;

        resistances.values[0] = n[6];
        resistances.values[0] = n[7];
        resistances.values[0] = n[8];
        resistances.values[0] = n[9];
    }
}

public struct Damages//cannot use reflection on struct?
{
    [Header("Damages")]
    public float[] values;
    public Damages(float thermal, float em, float kinetic, float mental)
    {
        values = new float[4];//float[System.Enum.GetValues(typeof(_DamageType)).Length];       

        //dynamic alternative for bellow? idk 
        values[(int)_DamageType.thermal] = thermal;
        values[(int)_DamageType.em] = em;
        values[(int)_DamageType.kinetic] = kinetic;
        values[(int)_DamageType.mental] = mental;
    }
}

public struct Buff
{
    [Header("Buff")]
    public bool isTemporary;//this var shouldnt really be a thing since u can just check if termEnddate is null but eh.
    public DateTime termEndDate;

    public math_func_indic effectStyle;
    public EntityValues effect; 
    
    public Buff(bool isTemporary, DateTime termEndDate, math_func_indic effectStyle, EntityValues effect)
    {
        this.isTemporary = isTemporary;
        this.termEndDate = isTemporary ? termEndDate : DateTime.MaxValue;
        this.effectStyle = effectStyle;
        this.effect = effect;
    }
}

public struct Buffmanager
{
    [Header("Buff Manager")]
    static Hashtable stylePointers = new Hashtable()
    {
        {(math_func_indic.add, math_func_indic.subtract), effValByStyle.add},//multiply all by -1 for subtract
        {math_func_indic.multiply, effValByStyle.multiply},
        {(math_func_indic.greatestOf, math_func_indic.leastOf, math_func_indic.difference, math_func_indic.mean), effValByStyle.other},
        {(math_func_indic.combineModiPerc, math_func_indic.removeModiPerc), effValByStyle.modiPerc}
    };
    enum effValByStyle//effective values by style. this is to help manage Buffs being added and removed by updating as little as possiable instead of recalculating everyhting
    {
        add,
        multiply,
        modiPerc,
        other//does not get its own list. make sure to keep this as the last enum. everyhting under this catagory will alwayse be recalculated every time somthing is changed
    }

    public List<Buff> _buffs;

    public EntityValues eVals_base;
    
    //protected EntityValues effectiveValue = new EntityValues(); //<- protected field in a struct not supported by c#9, only in c#10+ .... 
    public EntityValues effectiveValues { get { return _effEV; } }
    private EntityValues _effEV;

    private EntityValues[] _effectiveValues;

    public void addBuff(Buff b)
    {
        _buffs.Add(b);

        int styleType = (int)(effValByStyle)stylePointers[b.effectStyle];//idk is the double cast is necessary but think so because might unbox differently
        
        if(styleType != (int)effValByStyle.other)//the 'other' type are on the bottom of the queues
        {
            if (_effectiveValues[styleType] == null)
                _effectiveValues[styleType] = b.effect;
            else
                _effectiveValues[styleType].setVars(Stuff.combineArr(b.effectStyle, _effectiveValues[styleType].getVars(), b.effect.getVars()));
        }

        calcEffectiveValue();
    }
    public void removeBuff(Buff b)
    {
        if (_buffs.Remove(b))
            return;

        int styleType = (int)(effValByStyle)stylePointers[b.effectStyle];

        if (styleType != (int)effValByStyle.other)
        {
            try
            {
                _effectiveValues[styleType].setVars(
                        Stuff.reverseCombineArr(b.effectStyle, _effectiveValues[styleType].getVars(), b.effect.getVars())//this could return null if operation cannot be reversed
                    );
            }
            catch
            {
                //recalculates a new effectiveValue for the style type
                _effectiveValues[styleType] = eVals_base;
                foreach (Buff o in _buffs)
                    if ((int)(effValByStyle)stylePointers[o.effectStyle] == styleType)
                        _effectiveValues[styleType].setVars(Stuff.combineArr(o.effectStyle, _effectiveValues[styleType].getVars(), o.effect.getVars()));
            }
        }

        calcEffectiveValue();
    }

    public void clearAll()
    {
        _buffs.Clear();
        for (int i = 0; i < _effectiveValues.Length; i++)
            _effectiveValues[i] = null;

        calcEffectiveValue();
    }

    public void recalcAll()
    {
        _effEV = eVals_base;

        for (int i = 0; i < _effectiveValues.Length; i++)
            _effectiveValues[i] = null;

        Queue<int> otherIdxs = new Queue<int>();

        for (int i = 0; i < _buffs.Count; i++)
        {
            effValByStyle styleType = (effValByStyle)stylePointers[_buffs[i].effectStyle];
            if (styleType == effValByStyle.other)
                otherIdxs.Enqueue(i);
            else if (_effectiveValues[(int)styleType] != null)
                _effectiveValues[(int)styleType].setVars(Stuff.combineArr(_buffs[i].effectStyle, _effectiveValues[(int)styleType].getVars(), _buffs[i].effect.getVars()));
            else
                _effectiveValues[(int)styleType] = _buffs[i].effect;
        }

        while(otherIdxs.Count > 0)//!!!danger!!! D: 
        {
            Buff b = _buffs[otherIdxs.Dequeue()];
            _effEV.setVars(Stuff.combineArr(b.effectStyle, _effEV.getVars(), b.effect.getVars()));
        }
    }

    public void calcEffectiveValue()//shouldnt be called outside of this class aside after the eV_base values are changed for what ever reason
    {
        _effEV = eVals_base;
        
        if (_effectiveValues[(int)effValByStyle.add] != null)
            _effEV.setVars(Stuff.combineArr(math_func_indic.add, _effEV.getVars(), _effectiveValues[(int)effValByStyle.add].getVars()));

        if (_effectiveValues[(int)effValByStyle.multiply] != null)
            _effEV.setVars(Stuff.combineArr(math_func_indic.multiply, _effEV.getVars(), _effectiveValues[(int)effValByStyle.multiply].getVars()));

        if (_effectiveValues[(int)effValByStyle.modiPerc] != null)
            _effEV.setVars(Stuff.combineArr(math_func_indic.combineModiPerc, _effEV.getVars(), Stuff.combineArr(math_func_indic.add, _effectiveValues[(int)effValByStyle.modiPerc].getVars(), 1)));

        //other are after effects applyed by order
        foreach(Buff b in _buffs.Where(buff => (effValByStyle)stylePointers[buff.effectStyle] == effValByStyle.other))
            _effEV.setVars(Stuff.combineArr(b.effectStyle, _effEV.getVars(), b.effect.getVars()));
    }

    public Buffmanager(EntityValues baseValues)
    {
        _buffs = new List<Buff>();
        eVals_base = baseValues;
        _effEV = eVals_base;
        _effectiveValues = new EntityValues[Enum.GetValues(typeof(effValByStyle)).Length - 1];//the -1 is since the 'others' field dosent gte its own effect as its a after effect that applies to the whole
    }
}

