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
  protected showProjectDesc: boolean = false
  protected showBugImage: boolean = false


  protected previewUrl: string | ArrayBuffer | null = null;
  protected selectedFile: File | null = null;

  protected previewUrlEdit: string | ArrayBuffer | null = null;
  protected selectedFileEdit: File | null = null;



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
    this.previewUrl = null;
    this.selectedFile = null;

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

  openProjectDesc() {
    this.showProjectDesc = true;
    this.disableScroll()
  }

  closeProjectDesc() {
    this.showProjectDesc = false
    this.enableScroll()
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement
    if (input.files && input.files[0]) {
      this.selectedFile = input.files[0]
      const reader = new FileReader()
      reader.onload = () => this.previewUrl = reader.result
      reader.readAsDataURL(this.selectedFile)
    }
  }

    onFileSelectedEdit(event: Event) {
    const input = event.target as HTMLInputElement
    if (input.files && input.files[0]) {
      this.selectedFileEdit = input.files[0]
      const reader = new FileReader()
      reader.onload = () => this.previewUrlEdit = reader.result
      reader.readAsDataURL(this.selectedFileEdit)
    }
  }

  openBugImage(){
    this.showBugImage = true;
    this.enableScroll();
  }

  closeBugImage(){
    this.showBugImage = false;
    this.disableScroll()
  }


}
