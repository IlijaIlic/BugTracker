import { BugModel } from "./bug.model";
import { ManagerModel } from "./manager.model";

type Status = "Planning" | "Active" | "Blocked"

export interface ProjectModel{
    id: number;
    name: string;
    description: string;

    owner: ManagerModel;

    status: Status;
    bugs: BugModel[];
}