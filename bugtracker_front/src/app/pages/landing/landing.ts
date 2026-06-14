import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-landing',
  imports: [RouterModule],
  templateUrl: './landing.html',
  styleUrl: './landing.scss',
})
export class Landing implements OnInit {

  protected authService = inject(AuthService)
  protected router = inject(Router)

  ngOnInit(): void {
    if(this.authService.currentUser()){
      this.router.navigate(['/projects'])
    }
  }
}
