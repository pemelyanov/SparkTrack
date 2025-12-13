export interface Feature {
  id: number;
  name: string;
  project: string;
  deadline: Date;
  status: string;
}

// public int Id { get; init; }
//
// public required string Name { get; init; }
//
// public required Project Project { get; init; }
//
// public required IReadOnlyList<SubTask> TasksList { get; init; }
//
// public DateTime Deadline { get; init; }
//
// public string Description { get; init; } = string.Empty;
//
// public IReadOnlyList<FileInfo> AttachmentsList { get; init; } = [];
