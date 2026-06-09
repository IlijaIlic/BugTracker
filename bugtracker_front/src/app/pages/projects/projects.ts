import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Combobox } from '../../components/combobox/combobox';

@Component({
  selector: 'app-projects',
  imports: [FormsModule, Combobox],
  templateUrl: './projects.html',
  styleUrl: './projects.scss',
})
export class Projects {

  protected list: number[] = [1, 2, 3, 4, 5, 2, 3, 4, 5, 2, 3, 4, 5, 2, 3, 4, 5]
  protected selProject: any = null

  protected showYourProjects: boolean = false;
  protected showAddProject: boolean = false;
  protected showProjectMoreDetails: boolean = false;



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
    this.enableScroll()
  }

  projectMoreDetails(project: any) {
    this.showProjectMoreDetails = true


    console.log(project)
    this.selProject = project 
  }

  closeProjectMoreDetails() {
    this.showProjectMoreDetails = false

    this.selProject = null
  }

}
