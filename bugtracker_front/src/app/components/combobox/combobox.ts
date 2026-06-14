import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-combobox',
  imports: [FormsModule],
  templateUrl: './combobox.html',
  styleUrl: './combobox.scss',
})
export class Combobox implements OnInit{
  protected showMenu: boolean = false
  protected displayedText: string | null = null

  @Input() options: string[] = [];
  @Input() placeHolderText: string = 'Placeholder'
  @Input() startText?: string

  @Output() chosenText = new EventEmitter<string>();

  ngOnInit(): void {
    if(this.startText) this.setTextTo(this.startText)
  }
  toggleOpenMenu(){
    this.showMenu = !this.showMenu;
  }

  setTextTo(text: string){
    this.displayedText = text;
    this.showMenu = false;
    this.chosenText.emit(this.displayedText)
  }
  
}