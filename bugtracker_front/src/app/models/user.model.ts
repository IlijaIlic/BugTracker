import { BugModel } from "./bug.model";
import { ProjectModel } from "./project.model";

export interface UserProfileModel {
    id: string;
    username: string;
    email: string;
    role: 'Manager' | 'Tester';
    projects: ProjectModel[];   // empty if Tester
    bugs: BugModel[];
}

export interface UserEditModel{
    username: string;
    email: string;
}
