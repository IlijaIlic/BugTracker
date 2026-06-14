import { ProjectModel } from "./project.model";

export interface BugModel {
    id: string;
    name: string;
    description: string;

    dateAdded?: Date;
    dateFixed?: Date;

    status: string;
    severity: string;
    priority: string;
    platform: string;

    project: ProjectModel;

    imageUrl: string;
    ownerName: string;
}



export interface BugModelUpdate {
    name?: string;
    description?: string;
    dateFixed?: Date;
    severity?: string;
    priority?: string;
    platform?: string;
}