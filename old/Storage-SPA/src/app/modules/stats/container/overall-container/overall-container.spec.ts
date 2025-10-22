import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OverallContainer } from './overall-container';

describe('OverallContainer', () => {
  let component: OverallContainer;
  let fixture: ComponentFixture<OverallContainer>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [OverallContainer]
    })
    .compileComponents();

    fixture = TestBed.createComponent(OverallContainer);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
