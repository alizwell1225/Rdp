using Automation.BDaq;

namespace LIB_Machine.AdvenTech;

public class PCI1753
{
    private const int BIT_NUM_PERT_PORT = 8;
    private const int INPUT_START_PORT = 0;
    private const int OUTPUT_START_PORT = 0;
    private const int MAX_INPUT_PORT_NUM = 11;
    private const int MAX_OUTPUT_PORT_NUM = 1;

    private int DeviceNo;
    private int[] InputBit;
    private int[] InputPort;
    private int[] OutputBit;
    private int[] OutputPort;
    private InstantDiCtrl InputCtrl;
    private InstantDoCtrl OutputCtrl;
    private DeviceInformation devInfoCtrl;
    private string Description;

    public PCI1753(string description, int inputStartPort = INPUT_START_PORT, int inputPortNum = MAX_INPUT_PORT_NUM,
        int outputStartPort = OUTPUT_START_PORT, int outputPortNum = MAX_OUTPUT_PORT_NUM)
    {
        InputCtrl = new InstantDiCtrl();
        OutputCtrl = new InstantDoCtrl();
        Description = description;
        CreateMem(inputStartPort, inputPortNum, outputStartPort, outputPortNum);
    }

    ~PCI1753()
    {
    }

    private void CreateMem(int inputStartPort, int inputPortNum, int outputStartPort, int outputPortNum)
    {
        int i, num, bitIndex, portIndex;

        num = inputPortNum * BIT_NUM_PERT_PORT;
        InputPort = new int[num];
        InputBit = new int[num];
        bitIndex = 0;
        portIndex = inputStartPort;
        for (i = 0; i < num; i++)
        {
            InputPort[i] = portIndex;
            InputBit[i] = bitIndex;
            bitIndex++;
            if (bitIndex >= BIT_NUM_PERT_PORT)
            {
                bitIndex = 0;
                portIndex++;
            }
        }

        num = outputPortNum * BIT_NUM_PERT_PORT;
        OutputPort = new int[num];
        OutputBit = new int[num];
        bitIndex = 0;
        portIndex = outputStartPort;
        for (i = 0; i < num; i++)
        {
            OutputPort[i] = portIndex;
            OutputBit[i] = bitIndex;
            bitIndex++;
            if (bitIndex >= BIT_NUM_PERT_PORT)
            {
                bitIndex = 0;
                portIndex++;
            }
        }
    }

    public bool Open()
    {
        try
        {
            devInfoCtrl = new DeviceInformation();
            //var DeviceNumber = devInfoCtrl.DeviceNumber;
            //var DeviceMode = devInfoCtrl.DeviceMode;
            //var Description = devInfoCtrl.Description;

            if (InputCtrl.Initialized == false)
                InputCtrl.SelectedDevice = new DeviceInformation(Description);
            if (OutputCtrl.Initialized == false)
                OutputCtrl.SelectedDevice = new DeviceInformation(Description);
        }
        catch (Exception ee)
        {
            Console.WriteLine(ee);
            return false;
        }

        return IsOpened();
    }

    public bool IsOpened()
    {
        return InputCtrl.Initialized && OutputCtrl.Initialized;
    }

    public bool GetInput(int index)
    {
        if (InputCtrl.Initialized == false)
            return false;
        byte status;
        if (InputCtrl.ReadBit(InputPort[index], InputBit[index], out status) != ErrorCode.Success)
            return false;
        return status != 0;
    }

    public bool GetOutput(int index)
    {
        if (OutputCtrl.Initialized == false)
            return false;
        byte status;
        if (OutputCtrl.ReadBit(OutputPort[index], OutputBit[index], out status) != ErrorCode.Success)
            return false;
        return status != 0;
    }

    public bool SetOutput(int index, bool flag)
    {
        if (OutputCtrl.Initialized == false)
            return false;
        byte status = 0;
        if (flag) status = 1;
        if (OutputCtrl.WriteBit(OutputPort[index], OutputBit[index], status) == ErrorCode.Success)
            return true;
        return false;
    }
}