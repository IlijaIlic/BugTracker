import { Component, HostListener, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Combobox } from '../../components/combobox/combobox';
import { ActivatedRoute } from '@angular/router';
import { debounceTime, forkJoin, Subject, switchMap } from 'rxjs';
import { ProjectService } from '../../services/project.service';
import { ProjectModel } from '../../models/project.model';
import { BugModel, BugModelUpdate } from '../../models/bug.model';
import { BugService } from '../../services/bug.service';


@Component({
  selector: 'app-bugs',
  imports: [FormsModule, Combobox],
  templateUrl: './bugs.html',
  styleUrl: './bugs.scss',
})
export class Bugs implements OnInit {

  protected route = inject(ActivatedRoute)
  protected projectService = inject(ProjectService)
  protected bugService = inject(BugService)

  public imageApi = 'https://localhost:7236/'
  protected showYourBugs: boolean = false;
  protected showSubmitBug: boolean = false;
  protected showBugMoreDetails: boolean = false
  protected showProjectDesc: boolean = false
  protected showBugImage: boolean = false


  protected previewUrl: string | ArrayBuffer | null = null;
  protected selectedFile: File | null = null;

  protected previewUrlEdit: string | ArrayBuffer | null = null;
  protected selectedFileEdit: File | null = null;

  protected project: ProjectModel | null = null;
  protected myBugs: BugModel[] | null = null;
  protected allBugs: BugModel[] | null = null;
  protected newBug: BugModel = {
    id: -1,
    name: "",
    description: "",
    status: 'Active',
    severity: 'Low',
    priority: 'Low',
    platform: 'Android',
    project: this.project!,
    imageUrl: "",
    ownerName: ""
  };

  protected selBug: BugModel | null = null;
  protected selBugDate: string | null = null;
  protected selBugOriginalName: string = "";

  protected searchSubject = new Subject<string>();
  protected searchInput: string = ""

  protected projectId: number = 0;

  ngOnInit(): void {
    this.searchSubject.pipe(
      debounceTime(1000)
    ).subscribe(search => {
      console.log('debounced search fired:', search) // ← add this
      this.bugService.getBugsByProject(this.projectId, search).subscribe({
        next: (resp) => this.allBugs = resp,
        error: (err) => console.log(err)
      });
    });

    this.getAll();
  }

  getAll() {
    this.route.paramMap.pipe(
      switchMap(params => {
        this.projectId = Number(params.get('id'));
        return forkJoin({
          project: this.projectService.getById(this.projectId),
          allBugs: this.bugService.getBugsByProject(this.projectId, this.searchInput),
          myBugs: this.bugService.getMyBugs(this.projectId)
        });
      })
    ).subscribe({
      next: (resp) => {
        this.project = resp.project;
        this.allBugs = resp.allBugs;
        this.myBugs = resp.myBugs;
      },
      error: (err) => console.log(err)
    });
  }

  onSearch() {
    console.log(this.searchInput)
    this.searchSubject.next(this.searchInput);
  }

  dateToReadable(date: any) {
    const dateFormat = new Date(date);
    return dateFormat.toLocaleDateString("en-US", {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  handleSubmitBug() {
    const formData = new FormData();
    formData.append('Name', this.newBug!.name);
    formData.append('Platform', this.newBug!.platform);
    formData.append('Severity', this.newBug!.severity);
    formData.append('Priority', this.newBug!.priority);
    formData.append("ProjectId", this.project!.id.toString())
    if (this.newBug!.description) formData.append('Description', this.newBug!.description);
    if (this.selectedFile) formData.append('Image', this.selectedFile);

    this.bugService.addBug(formData).subscribe({
      next: (resp) => {
        console.log(resp)
        this.closeSubmitBug()
        this.getAll()
      },
      error: (err) => {
        alert(err.name)
        console.log(err)},
    })
  }

  handleUpdate() {
    const formData = new FormData();
    if (this.selBug!.name) formData.append('Name', this.selBug!.name);
    if (this.selBug!.platform) formData.append('Platform', this.selBug!.platform);
    if (this.selBug!.severity) formData.append('Severity', this.selBug!.severity);
    if (this.selBug!.priority) formData.append('Priority', this.selBug!.priority);
    if (this.selBug!.description) formData.append('Description', this.selBug!.description);
    if (this.selectedFileEdit) formData.append('Image', this.selectedFileEdit);
    if (this.selBugDate) formData.append('DateFixed', this.selBugDate);

    this.bugService.updateBug(formData, +this.selBug!.id).subscribe({
      next: (resp) => {
        console.log(resp)
        this.closeBugMoreDetails()
        this.getAll()
      },
      error: (err) =>{ console.log(err)
        alert(err.name)
      }
    })
  }

  handleDeleteBug() {
    this.bugService.deleteBug(this.selBug!.id).subscribe({
      next: (resp) => {
        
        console.log(resp)
        this.closeBugMoreDetails()
        this.getAll()
      },
      error: (err) =>{ 
        console.log(err)
        alert(err.name)
      }
    })
  }

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
    this.newBug = {
      id: -1,
      name: "",
      description: "",
      status: 'Active',
      severity: 'Low',
      priority: 'Low',
      platform: 'Android',
      project: this.project!,
      imageUrl: "",
      ownerName: ""
    }

    this.enableScroll()
  }

  bugMoreDetails(bug: BugModel) {
    this.showBugMoreDetails = true
       this.previewUrlEdit = null;
    this.disableScroll()


    console.log(bug)
    this.selBug = { ...bug }
    if (bug.dateFixed) this.selBugDate = new Date(bug.dateFixed).toISOString().split('T')[0]
    this.selBugOriginalName = bug.name
  }

  closeBugMoreDetails() {
    this.showBugMoreDetails = false
    this.enableScroll()

    this.selBug = null
    this.selBugDate = null
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

  openBugImage() {
    this.showBugImage = true;
    this.enableScroll();
  }

  closeBugImage() {
    this.showBugImage = false;
    this.disableScroll()
  }


}
