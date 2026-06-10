import { ProjectModel } from "./project.model";

type Status = "fixed" | "active";
type Severity = "Low" | "Moderate" | "Major" | "Critical";
type Priority = "Low" | "Medium" | "High";
type Platform = "Android" | "iOS" | "Web" | "Mobile Web";

export interface BugModel {
    id: string;
    name: string;
    description: string;

    dateAdded: Date;
    dateFixed: Date;

    status: Status;
    severity: Severity;
    priority: Priority;
    platform: Platform;

    project: ProjectModel;

    imageUrl: string;
}