import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PurchasesPresenter } from './purchases-presenter';

describe('PurchasesPresenter', () => {
  let component: PurchasesPresenter;
  let fixture: ComponentFixture<PurchasesPresenter>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [PurchasesPresenter]
    })
    .compileComponents();

    fixture = TestBed.createComponent(PurchasesPresenter);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
