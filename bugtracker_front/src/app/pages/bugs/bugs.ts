import { Component, HostListener } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Combobox } from '../../components/combobox/combobox';

@Component({
  selector: 'app-bugs',
  imports: [FormsModule, Combobox],
  templateUrl: './bugs.html',
  styleUrl: './bugs.scss',
})
export class Bugs {

  protected projectName: string = "YourProjectName";
  protected list: number[] = [1, 2, 3, 4, 5, 6, 7, 8, 9];
  protected selBug: any = null;

  protected showYourBugs: boolean = false;
  protected showSubmitBug: boolean = false;
  protected showBugMoreDetails: boolean = false



  disableScroll() {
    document.body.classList.add('overflow-h')
  }

  enableScroll() {
    document.body.classList.remove('overflow-h')
  }

  openYourBugsModal() {
    this.showYourBugs = true
    this.disableScroll()
  }

  closeYourBugsModal() {
    this.showYourBugs = false
    this.enableScroll()
  }

  openSubmitBug() {
    this.showSubmitBug = true
    this.disableScroll()
  }

  closeSubmitBug() {
    this.showSubmitBug = false
    this.enableScroll()
  }

  bugMoreDetails(project: any) {
    this.showBugMoreDetails = true
    this.disableScroll()


    console.log(project)
    this.selBug = project
  }

  closeBugMoreDetails() {
    this.showBugMoreDetails = false
    this.enableScroll()


    this.selBug = null
  }

}
