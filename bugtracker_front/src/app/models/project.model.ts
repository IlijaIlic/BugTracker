import { BugModel } from "./bug.model";


export interface ProjectModel {
    id: number;
    name: string;
    description: string;

    ownerName: string;

    status: string;
    bugs: BugModel[];
    bugsSum?: BugSumModel[];
    bugCount: number;
}

export interface AddProjectModel {
    name: string;
    description: string;
    status: string;
}

export interface BugSumModel {
    id: number;
    name: string;
    severity: string;
    status: string;
}
