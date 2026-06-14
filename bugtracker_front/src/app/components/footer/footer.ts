import { Component, inject, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-footer',
  imports: [],
  templateUrl: './footer.html',
  styleUrl: './footer.scss',
})
export class Footer implements OnInit {
  protected userName: string = "JohnDoe123"

  protected authService = inject(AuthService);
  protected router = inject(Router);
  protected user: any;

  ngOnInit(): void {
    this.userName = this.authService.currentUser() != null ? this.authService.currentUser()!.username : "NotLogged"
    this.user = this.authService.currentUser
  }

  handleLogout(){
    this.authService.logout();
    this.router.navigate(['/']);
  }

}
