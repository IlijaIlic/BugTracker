import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { Combobox } from '../../components/combobox/combobox';
import { AuthService } from '../../services/auth.service';
import { RegisterModel } from '../../models/auth.model';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-register',
  imports: [RouterModule, Combobox, FormsModule],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class Register implements OnInit {
  private authService = inject(AuthService)
  private router = inject(Router);

  protected registerData: RegisterModel = {
    Email: "",
    Password: "",
    reppassword: "",
    Role: "",
    Username: ""
  }

  ngOnInit(): void {
    if (this.authService.currentUser()) {
      this.router.navigate(['/projects'])
    }
  }

  handleRegister() {
    if (this.registerData.Password != this.registerData.reppassword) {
      alert("Passwords must match!")
      return
    }

    this.authService.register(this.registerData).subscribe({
      next: (resp) => {
        console.log(resp)
        this.router.navigate(["/projects"])
      },
      error: (err) => {
        console.log(err)
      alert(err);
      },
    })
  }

  handleChange(event:any){
    console.log(event)
  }

}
