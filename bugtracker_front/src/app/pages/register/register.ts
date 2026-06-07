import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { Combobox } from '../../components/combobox/combobox';

@Component({
  selector: 'app-register',
  imports: [RouterModule, Combobox],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class Register {

}
