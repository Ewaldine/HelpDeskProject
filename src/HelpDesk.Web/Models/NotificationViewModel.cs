using System;
using System.Collections.Generic;
using HelpDesk.Shared.DTOs;

namespace HelpDesk.Web.Models;

public class NotificationViewModel
{
    public List<NotificationDto> Notifications { get; set; } = new List<NotificationDto>();
}
