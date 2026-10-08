import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OverallPresenter } from './overall-presenter';

describe('OverallPresenter', () => {
  let component: OverallPresenter;
  let fixture: ComponentFixture<OverallPresenter>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [OverallPresenter]
    })
    .compileComponents();

    fixture = TestBed.createComponent(OverallPresenter);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
