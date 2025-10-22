/**
 * Liczba podziałek osi wartości dla małych liczb całkowitych.
 * Przy maksimum np. 2 ApexCharts dzieli oś na ułamki (0; 0,5; 1; 1,5; 2),
 * a zaokrąglone etykiety powtarzają się („0 1 1 2 2”). Wtedy stawiamy jedną
 * podziałkę na jednostkę; przy większych wartościach zostaje podział domyślny.
 */
export function integerTickAmount(maxValue: number): number | undefined {
  const max = Math.ceil(maxValue);
  return max > 0 && max <= 10 ? max : undefined;
}
