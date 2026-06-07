import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-combobox',
  imports: [FormsModule],
  templateUrl: './combobox.html',
  styleUrl: './combobox.scss',
})
export class Combobox {
  protected showMenu: boolean = false
  protected displayedText: string | null = null

  @Input() options: string[] = [];
  @Input() placeHolderText: string = 'Placeholder'


  toggleOpenMenu(){
    console.log("TEST")
    this.showMenu = !this.showMenu;
  }

  setTextTo(text: string){
    this.displayedText = text;
    this.showMenu = false;
  }
  
}