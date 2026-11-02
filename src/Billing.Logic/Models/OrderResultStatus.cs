namespace Billing.Logic.Models;

public enum OrderResultStatus
{
    Processed,
    Duplicate,
    UnknownGateway,
    Declined
}
