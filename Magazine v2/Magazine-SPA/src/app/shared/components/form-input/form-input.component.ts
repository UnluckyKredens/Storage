import { booleanAttribute, Component, forwardRef, Input } from '@angular/core';
import {
  ControlValueAccessor,
  NG_VALIDATORS,
  NG_VALUE_ACCESSOR,
  ValidationErrors,
  Validator,
} from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-form-input',
  imports: [MatFormFieldModule, MatInputModule],
  templateUrl: './form-input.component.html',
  styleUrl: './form-input.component.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => FormInputComponent),
      multi: true,
    },
    {
      provide: NG_VALIDATORS,
      useExisting: forwardRef(() => FormInputComponent),
      multi: true,
    },
  ],
})
export class FormInputComponent implements ControlValueAccessor, Validator {
  @Input() label = '';
  @Input() type = 'text';
  @Input() placeholder = '';
  @Input() autocomplete = '';
  @Input() min: number | string | null = null;
  @Input() max: number | string | null = null;
  @Input() step: number | string | null = null;
  @Input() minLength: number | string | null = null;
  @Input() maxLength: number | string | null = null;
  @Input({ transform: booleanAttribute }) required = false;
  @Input({ transform: booleanAttribute }) disabled = false;

  value: string | number | null = '';

  private onChange: (value: string | number | null) => void = () => undefined;
  private onTouched: () => void = () => undefined;

  writeValue(value: string | number | null): void {
    this.value = value ?? '';
  }

  registerOnChange(fn: (value: string | number | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(disabled: boolean): void {
    this.disabled = disabled;
  }

  changeValue(event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = this.type === 'number' ? this.numberValue(input.value) : input.value;
    this.value = value;
    this.onChange(value);
  }

  touch(): void {
    this.onTouched();
  }

  validate(): ValidationErrors | null {
    if (this.disabled) return null;

    const text = this.value === null || this.value === undefined ? '' : String(this.value);
    if (this.required && text.trim() === '') return { required: true };
    if (text === '') return null;
    if (this.type === 'email' && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(text)) return { email: true };

    const minLength = this.toNumber(this.minLength);
    if (minLength !== null && text.length < minLength) return { minlength: true };

    const maxLength = this.toNumber(this.maxLength);
    if (maxLength !== null && text.length > maxLength) return { maxlength: true };

    if (this.type !== 'number') return null;

    const value = Number(this.value);
    const min = this.toNumber(this.min);
    if (min !== null && value < min) return { min: true };

    const max = this.toNumber(this.max);
    if (max !== null && value > max) return { max: true };

    return null;
  }

  private numberValue(value: string): number | null {
    return value === '' ? null : Number(value);
  }

  private toNumber(value: number | string | null): number | null {
    if (value === null || value === '') return null;
    const number = Number(value);
    return Number.isNaN(number) ? null : number;
  }
}
