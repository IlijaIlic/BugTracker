import { Component, inject, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
import { ProjectModel } from '../../models/project.model';
import { BugModel } from '../../models/bug.model';
import { UserService } from '../../services/user.service';
import { UserEditModel, UserProfileModel } from '../../models/user.model';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-profile',
  imports: [FormsModule],
  templateUrl: './profile.html',
  styleUrl: './profile.scss',
})
export class Profile implements OnInit {

  protected authService = inject(AuthService)
  protected userService = inject(UserService)
  protected router = inject(Router)

  protected showMyBugs = false;
  protected showMyProjects = false;

  protected user: UserProfileModel | null = null;
  protected editUser: UserEditModel = {
    email: "",
    username: ""
  };


  ngOnInit(): void {
    if (this.authService.currentUser() == null) {
      this.router.navigate(['/'])
    }

    this.userService.getMe().subscribe({
      next: (resp) => {
        console.log(resp)
        this.user = resp
        this.editUser.email = resp.email
        this.editUser.username = resp.username
      },

      error: (err) => console.log(err),
    })

  }

  handleNavigateToProject(proj: ProjectModel) {
    this.router.navigate(['/bugs/', proj.id])
  }

  openMyBugs(){
    this.showMyBugs = true
  }

  closeMyBugs(){
    this.showMyBugs = false
  }

  openMyProjects(){
    this.showMyProjects = true
  }

  closeMyProjects(){
    this.showMyProjects = false
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

  handleDeleteAccount() {
    this.userService.deleteMe().subscribe({
      next: (resp) => {
        console.log(resp)
        this.authService.logout()
        this.router.navigate(["/"])
      },
      error: (err) => {
        alert(err)
        console.log(err)
      }
    })
  }

  handleUpdateAccount() {
    this.userService.updateMe(this.editUser).subscribe({
      next: (resp) => {
        console.log(resp)
        location.reload()
      },
      error: (err) => {
        alert(err)
      }
    })
  }

}
