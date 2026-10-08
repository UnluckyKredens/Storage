import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SellersPresenter } from './sellers-presenter';

describe('SellersPresenter', () => {
  let component: SellersPresenter;
  let fixture: ComponentFixture<SellersPresenter>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SellersPresenter]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SellersPresenter);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
