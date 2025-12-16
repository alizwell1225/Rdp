using LIB_GUI;
using LIB_HTool.HMachine;
using System.Xml.Linq;
using LIB_Machine.AdvenTech;

namespace LIB_Machine;

public class IOController : Machine
{
    private AppRegistery AppRegCtrl;
    private int InputNum = 16;
    private int OutputNum = 16;
    private PCI1753 IoCtrl;
    private bool[] OutputSetFlag;
    private string Description = "";

    public IOController(string name, AppRegistery regCtrl) : base(0, name)
    {
        AppRegCtrl = regCtrl;
        Description = name;
        initIO();
    }

    private void initIO()
    {
        IO_ADDRESS_FLAG = false;
        IO_INPUT_NUM = InputNum;
        IO_OUTPUT_NUM = OutputNum;

        if (IO_INPUT_NUM < 0 || OutputNum < 0)
            throw new Exception("IO Card Error");
        OutputSetFlag=new bool[IO_OUTPUT_NUM];
        IoCtrl =new PCI1753(Description);
    }

    public bool VirtualFlag { get; set; } = false;
    //============ I/O ============
    public override bool SetIOOutput(int index, bool flag)
    {
        if (VirtualFlag)
        {
            OutputSetFlag[index] = flag;
            return true;
        }
        return IoCtrl.SetOutput(index, flag);
    }

    public override bool GetIOOutput(int index)
    {
        if (VirtualFlag)
            return OutputSetFlag[index];
        return IoCtrl.GetOutput(index);
    }


    public override bool GetIOInput(int index)
    {
        return IoCtrl.GetInput(index);
    }

    public override bool Open()
    {
        bool okflag = true;
        if (IoCtrl.Open() == false)
            return false;
        return okflag;
    }

    public override bool Close()
    {
        return true;
    }

    public override bool IsOpened()
    {
        return IoCtrl.IsOpened();
    }

    public override int GetAcc(int axisIndex)
    {
        return 0;
    }

    public override int GetDec(int axisIndex)
    {
        return 0;
    }

    public override int GetSpeed(int axisIndex)
    {
        return 0;
    }

    public override int GetApproachSpeed(int axisIndex)
    {
        return 0;
    }

    public override int GetCreepSpeed(int axisIndex)
    {
        return 0;
    }

    public override double GetHomeOffset(int axisIndex)
    {
        return 0;
    }

    public override double GetStepDistance(int axisIndex)
    {
        return 0;
    }

    public override bool SetAcc(int axisIndex, int acceleration)
    {
        return true;
    }

    public override bool SetDec(int axisIndex, int deceleratiom)
    {
        return true;
    }

    public override bool SetSpeed(int axisIndex, int speed)
    {
        return true;
    }

    public override bool SetApproachSpeed(int axisIndex, int speed)
    {
        return true;
    }

    public override bool SetCreepSpeed(int axisIndex, int speed)
    {
        return true;
    }

    public override bool SetHomeOffset(int axisIndex, double distance)
    {
        return true;
    }

    public override bool SetStepDistance(int axisIndex, double distance)
    {
        return true;
    }

    public override double GetCoordinate(int axisIndex)
    {
        return 0;
    }

    public override bool GetCoordinate(int axisIndex, ref double coord)
    {
        return true;
    }

    public override bool ServoIsON(int axisIndex)
    {
        return false;
    }

    public override bool IsHomeFinished(int axisIndex)
    {
        return false;
    }

    public override bool GetServoIO(int axisIndex, ref bool zeroFlag, ref bool pLFlag, ref bool nLFlag)
    {
        return false;
    }

    public override bool IsStoped(int axisIndex)
    {
        return false;
    }

    public override bool IsMoveFinished(int axisIndex)
    {
        return false;
    }

    public override bool IsMoveReached(int axisIndex)
    {
        return false;
    }

    public override int GetAlarm(int axisIndex)
    {
        return 0;
    }

    public override int GetWarning(int axisIndex)
    {
        return 0;
    }

    public override bool StopMove(int axisIndex)
    {
        return true;
    }

    public override bool ClearAlarm(int axisIndex)
    {
        return true;
    }

    public override bool ServoON(int axisIndex)
    {
        return false;
    }

    public override bool ServoOFF(int axisIndex)
    {
        return false;
    }

    public override bool MoveHome(int axisIndex)
    {
        return true;
    }

    public override bool Move(int axisIndex, double coord)
    {
        return true;
    }

    public override bool Step(int axisIndex, double distance)
    {
        return true;
    }

    public override bool JogPositive(int axisIndex)
    {
        return true;
    }

    public override bool JogNegative(int axisIndex)
    {
        return true;
    }

    public override int GetAxisAlarmBitNum()
    {
        return 0;
    }

    public override int GetAxisWarningBitNum()
    {
        return 0;
    }

    public override string GetAxisAlarmStr(int axisIndex)
    {
        return "";
    }

    public override string GetAxisWarningStr(int axisIndex)
    {
        return "";
    }

    public override int GetHomeType(int axisIndex)
    {
        return 0;
    }

    public override bool SetHomeType(int axisIndex, int type)
    {
        return true;
    }
}