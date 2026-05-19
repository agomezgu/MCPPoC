using System.ComponentModel;
using System.Text.Json;
using AG.Mcp.Server.TimeOff;
using AG.Mcp.Server.Calendar;
using AG.Mcp.Server.Documents;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace AG.Mcp.Server.TimeOff;

[McpServerToolType]
public class PlanTimeOffTool(IHrmAbsenceApi hrmAbsenceApi, IHrmDocumentService hrmDocumentService)
{
    private readonly IHrmAbsenceApi hrmAbsenceApi = hrmAbsenceApi;
    private readonly IHrmDocumentService hrmDocumentService = hrmDocumentService;

    [McpServerTool(Title = "Plan Time Off"),  Description(
        """
        Help L&Lemployees plan and answer questions about time off.
        The tool includes the employee's scheduled time off calendar and the office location calendars.
        Prompt for their employee ID unless they provided it already.
        This will help you avoid scheduling time off during company holidays or overlapping with existing time off.
        This can also help answer questions about upcoming work holidays for the current year.
        You can use this tool to answer questions about time off, absence, and leave policies.
        If the tool result is not supported or is missing policy information, NEVER try to assume and inform the user that you cannot provide an answer,
        and that they may need to contact HR for further assistance.
        """)]
    public async Task<IEnumerable<ContentBlock>> PlanTimeOff(
        [Description("Provided by the user")] string employeeId,
        CancellationToken cancellationToken)
    {
        var employeeCalendarResource = await CalendarResources.EmployeeCalendarResource(employeeId, hrmAbsenceApi, cancellationToken);
        var employeeDetails = await hrmAbsenceApi.GetWorkerByIdAsync(employeeId, cancellationToken);
        var eligibility = await hrmAbsenceApi.GetEligibleAbsenceTypesAsync(employeeId, "not_used", cancellationToken);
        // var timeOffPolicyDocumentResource = await DocumentResources.DocumentResourceById(
        //     "L&L _Vacation_TimeOff_Policy.pdf", hrmDocumentService, cancellationToken);
        var documentData = await hrmDocumentService.GetBenefitPlanDocumentContentAsPlainTextAsync("L&L _Vacation_TimeOff_Policy.pdf", cancellationToken);

        return [
            new TextContentBlock
            {
                Text = $"Employee details: {JsonSerializer.Serialize(employeeDetails, McpJsonUtilities.DefaultOptions)}"
            },
            new TextContentBlock
            {
                Text = "You can find the L&Lwork and employee calendar(s) below for planning time off work."
            },
            new EmbeddedResourceBlock
            {
                Resource = new TextResourceContents()
                {
                    Uri = CalendarResources.ResourceWorkCalendarUri,
                    MimeType = "application/json",
                    Text = CalendarResources.WorkCalendarsResource(),
                }
            },
            new EmbeddedResourceBlock
            {
                Resource = new TextResourceContents()
                {
                    Uri = CalendarResources.ResourceEmployeeCalendarUri.Replace("{employeeId}", employeeId),
                    MimeType = "application/json",
                    Text = employeeCalendarResource,
                }
            },
            new TextContentBlock
            {
                Text = $"The employee meets all eligibility requirements and can request these types of time off: {
                    string.Join(", ", eligibility.AbsenceTypes.Select(at => at.ToTimeOffRequestType()))}"
            },
            new TextContentBlock
            {
                Text = "The L&LTime Off Policy details are below:"
            },
            // new ResourceLinkBlock
            // {
            //     Name = "L&LTime Off Policy Document",
            //     Uri = timeOffPolicyDocumentResource.Uri,
            //     MimeType = timeOffPolicyDocumentResource.MimeType
            // },
            // new EmbeddedResourceBlock
            // {
            //     Resource = timeOffPolicyDocumentResource           
            // },
            new TextContentBlock
            {
                Text = documentData
            },
        ];
    }
}