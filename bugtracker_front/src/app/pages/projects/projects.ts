import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Combobox } from '../../components/combobox/combobox';
import { ProjectService } from '../../services/project.service';
import { AddProjectModel, ProjectModel } from '../../models/project.model';
import { Router } from '@angular/router';
import { debounceTime, Subject } from 'rxjs';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-projects',
  imports: [FormsModule, Combobox],
  templateUrl: './projects.html',
  styleUrl: './projects.scss',
})
export class Projects implements OnInit {

  protected projectService = inject(ProjectService);
  protected authService = inject(AuthService);
  protected router = inject(Router);

  protected showYourProjects: boolean = false;
  protected showAddProject: boolean = false;
  protected showProjectMoreDetails: boolean = false;

  protected searchSubject = new Subject<string>();
  protected searchInput: string = ""

  protected addProject: AddProjectModel = {
    description: "",
    name: "",
    status: "Active"
  }

  protected projects: ProjectModel[] = [];
  protected myProjects: ProjectModel[] = [];

  protected selProject: ProjectModel | null = null
  protected selProjectTitle: string = ""


  ngOnInit(): void {
    this.searchSubject.pipe(
      debounceTime(1000)
    ).subscribe(search => {
      this.projectService.getAll(search).subscribe({
        next: (resp) => this.projects = resp,
        error: (err) => console.log(err)
      });
    });

    this.getProjects();
  }

  onSearch() {
    this.searchSubject.next(this.searchInput);
  }

  getProjects() {
    this.projectService.getAll(this.searchInput).subscribe({
      next: (resp) => {
        this.projects = resp
        console.log(resp)
      },
      error: (err) => console.log(err)
    })

    this.projectService.getMine().subscribe({
      next: (resp) => {
        this.myProjects = resp
        console.log(resp)
      },
      error: (err) => console.log(err)
    })

  }

  handleAddProject() {
    this.projectService.addProject(this.addProject).subscribe({
      next: (resp) => {
        console.log(resp)
        this.closeAddProject()
        this.getProjects()
      },
      error: (err) => {
        console.log(err)
        alert(err.error)
      }
    });
  }

  handleEditProject() {
    const updateData: AddProjectModel = {
      description: this.selProject!.description,
      name: this.selProject!.name,
      status: this.selProject!.status,
    }

    this.projectService.updateProject(updateData, this.selProject!.id).subscribe({
      next: (resp) => {
        console.log(resp)
        this.getProjects()
        this.closeProjectMoreDetails()
      },
      error: (err) => {
        alert(err)
        console.log(err)
      }
    });
  }

  handleDeleteProject() {
    this.projectService.deleteProject(this.selProject!.id).subscribe({
      next: (resp) => {
        console.log(resp)
        this.closeProjectMoreDetails()
        this.getProjects()
      },
      error: (resp) => console.log(resp)
    })
  }

  handleProjectStatus(event: any) {
    this.addProject.status = event
  }

  handleNavigateToProject(proj: ProjectModel) {
    this.router.navigate(['/bugs/', proj.id])
  }

  disableScroll() {
    document.body.classList.add('overflow-h')
  }

  enableScroll() {
    document.body.classList.remove('overflow-h')
  }

  openYourProjects() {
    this.showYourProjects = true
    this.disableScroll()
  }

  closeYourProjects() {
    this.showYourProjects = false
    this.enableScroll()
  }

  openAddProject() {
    this.showAddProject = true;
    this.disableScroll()
  }

  closeAddProject() {
    this.showAddProject = false;
    this.addProject = {
      description: "",
      name: "",
      status: "Active"
    }
    this.enableScroll()
  }

  projectMoreDetails(project: ProjectModel) {
    this.showProjectMoreDetails = true


    console.log(project)
    this.selProject = { ...project }
    this.selProjectTitle = project.name
  }

  closeProjectMoreDetails() {
    this.showProjectMoreDetails = false

    this.selProject = null
  }

}
