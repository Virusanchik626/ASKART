using Askart.Models;

namespace Askart.ViewModels;

public sealed record KanbanColumnItem(KanbanColumn Column, List<TaskItem> Tasks);
