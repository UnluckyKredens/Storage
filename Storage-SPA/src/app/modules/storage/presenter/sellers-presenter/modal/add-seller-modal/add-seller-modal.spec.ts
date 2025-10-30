import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddSellerModal } from './add-seller-modal';

describe('AddSellerModal', () => {
  let component: AddSellerModal;
  let fixture: ComponentFixture<AddSellerModal>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [AddSellerModal]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddSellerModal);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
