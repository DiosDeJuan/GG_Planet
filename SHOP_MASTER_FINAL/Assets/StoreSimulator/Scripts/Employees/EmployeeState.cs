//Adaptado por POMPIC 20100333
namespace FLOBUK.StoreSimulator
{
    public sealed class EmployeeState
    {
        public string employeeId;
        public bool isHired;
        public EmployeeRole role;
        public string assignedWorkstationId;

        public EmployeeState(string employeeId)
        {
            this.employeeId = employeeId;
            role = EmployeeRole.None;
            assignedWorkstationId = string.Empty;
        }
    }
}
