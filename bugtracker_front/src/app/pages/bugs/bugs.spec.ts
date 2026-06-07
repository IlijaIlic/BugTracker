import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Bugs } from './bugs';

describe('Bugs', () => {
  let component: Bugs;
  let fixture: ComponentFixture<Bugs>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Bugs]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Bugs);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
