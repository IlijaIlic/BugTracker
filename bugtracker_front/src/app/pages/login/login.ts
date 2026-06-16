import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { LoginModel } from '../../models/auth.model';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-login',
  imports: [RouterModule, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login implements OnInit {
  protected authService = inject(AuthService)
  protected router = inject(Router)

  protected loginData: LoginModel = {
    Email: "",
    Password: ""
  }

  ngOnInit(): void {
    if (this.authService.currentUser()) {
      this.router.navigate(['/projects'])
    }
  }

  handleLogin() {
    this.authService.login(this.loginData).subscribe({
      next: (resp) => {
        console.log(resp)
        this.router.navigate(['/projects'])
      },
      error: (err) => {
        console.log(err)
        alert("Login error: " + err.error);
      }
    })
  }
}
