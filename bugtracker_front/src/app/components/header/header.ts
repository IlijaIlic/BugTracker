import { Component, computed, inject, OnInit } from '@angular/core';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-header',
  imports: [RouterModule, FormsModule],
  templateUrl: './header.html',
  styleUrl: './header.scss',
})
export class Header {
  protected authService = inject(AuthService);
  protected userLogged = computed(() => this.authService.currentUser() != null);
}
