# AquaClinic — Casos de uso y autogestión del tiempo

> Documento para revisión con el cliente. Describe qué puede hacer hoy cada tipo de usuario
> y enumera las decisiones de negocio pendientes, cada una con el comportamiento actual del
> sistema (verificado en el código) para que la conversación parta de hechos y no de supuestos.

---

## 1. Actores

| Actor | Cómo entra | Qué busca |
|---|---|---|
| **Visitante (invitado)** | Sin cuenta, desde la web pública | Agendar un tratamiento sin fricción |
| **Cliente registrado** | Con correo y contraseña | Ver y seguir sus citas |
| **Personal (Recepción, Esteticista, Administrador)** | Login con rol | Manejar la agenda del día y el catálogo |

---

## 2. Casos de uso — el cliente autogestiona su tiempo

### CU-01 · Explorar el catálogo sin registrarse
El visitante ve especialidades, tratamientos, duración y precio en CAD sin crear cuenta
ni iniciar sesión. La decisión de reservar no exige datos personales por adelantado.

### CU-02 · Reservar su propia cita (invitado) — el caso central
1. Elige el tratamiento (la duración se ajusta sola al calendario).
2. Ve el calendario semanal **en tiempo real**: horas ocupadas, las de otros en proceso y las libres.
3. Hace clic en la hora que le conviene — la que él decide, no la que le asignan.
4. Completa un formulario corto (nombre, correo, teléfono, notas opcionales).
5. Recibe la confirmación por correo, con reintentos automáticos si el proveedor falla.

**Nada de llamadas ni idas y vueltas de WhatsApp para "ver disponibilidad": el calendario ES la disponibilidad.**

### CU-03 · Reserva suave — su hora no se la quita nadie
Mientras el visitante completa sus datos, su slot queda marcado como **"Reservando"**
(en dorado) y los demás visitantes lo ven ocupado durante ~2 minutos. Nadie puede
tomarle la hora en el último segundo.

### CU-04 · Sugerencias automáticas si su hora ya no está
Si el slot que eligió se ocupó mientras completaba sus datos (o no cabe la duración completa),
el modal le propone **horarios alternativos** — incluyendo el día siguiente — en pastillas de
un clic. El usuario no vuelve a empezar: elige una pastilla y continúa.

### CU-05 · Cliente registrado: sus citas, siempre a la vista
Quien tiene cuenta ve **"Mis citas"**: cada cita con servicio, fecha/hora completa, precio y
estado (Pendiente / Confirmada / Cancelada / Completada). Su historial queda registrado,
no en su memoria.

### CU-06 · Tiempo real para todos
La agenda usa SignalR: lo que reserva un visitante aparece al instante en el calendario del
personal (y viceversa). El contador "reservando ahora" late en vivo en la agenda interna.

---

## 3. Casos de uso — el personal protege y ordena el tiempo

### CU-07 · Agenda interna unificada
Calendario (día/semana/mes) con los estados por color + listado paginado de citas. Leyenda clara:
teal = cita agendada, dorado = alguien está reservando ahora mismo.

### CU-08 · Cargar una cita que llegó por teléfono o presencial
"Nueva cita" permite buscar clientes registrados o capturar un cliente no registrado, con
validación de choques de horario igual que la reserva pública.

### CU-09 · Exportar la agenda
La lista de citas se exporta a CSV (excel-friendly) para reportes o respaldo.

### CU-10 · Horarios y bloqueos respetados
El sistema solo ofrece horas dentro del horario de la clínica (Lun–Vie 9–18, Sáb 10–16,
domingo cerrado) y respeta los bloqueos por feriados/vacaciones de la esteticista.
El cliente **no puede** agendar a una hora que la clínica no atiende.

### CU-11 · Contabilidad automática de servicios atendidos (worker)
Cada cita confirmada cuya hora ya pasó se registra sola en la mini-contabilidad
(movimientos por cita×servicio con precio snapshot + resumen diario por servicio).
El dinero del día/mes se va contando solo, sin que nadie cargue nada.

---

## 4. Preguntas para el cliente

Cada pregunta indica **qué hace el sistema hoy** (verificado en el código) para que la
decisión sea informada.

### P1 · Estado de las citas al crearse ⭐ (la que planteaste)
- **Hoy:** toda cita —pública o interna— se crea **directamente en `Confirmada`**. No existe
  flujo de aceptación: ni el cliente ni el personal "aceptan" nada.
- **Decisión pendiente:** ¿debe seguir así (auto-confirmada, cero fricción) o quieres un
  flujo donde la recepción **acepte/rechace** cada solicitud antes de que quede firme?
  (La segunda requiere: estado `Pendiente` visible, bandeja de aprobación y correo de
  "cita aprobada" — hoy el correo dice confirmación.)

### P2 · Cancelar y reagendar desde la app
- **Hoy:** el cliente **no puede** cancelar ni mover su cita; debe llamar/escribir. El
  personal tampoco tiene botón de cancelar (el estado `Cancelada` existe en el esquema
  pero nadie lo produce).
- **Decisión:** ¿habilitamos auto-cancelación y auto-reagendamiento por el cliente? Con
  qué anticipación mínima (ej. no permitir cancelar 2 h antes)? ¿El reagendar reusa la
  reserva suave o es cancelar + crear?

### P3 · Marcar citas como Completadas
- **Hoy:** nadie puede marcar `Completada`; el estado existe pero es inalcanzable. La
  contabilidad (CU-11) cuenta las `Confirmadas` cuya hora pasó, así que hoy funciona sin
  ese botón.
- **Decisión:** ¿el esteticista cierra su cita al terminar la sesión (un clic en la agenda)?
  ¿Ese cierre debería ser la señal contable "real" en lugar de la hora transcurrida?

### P4 · Recordatorios antes de la cita
- **Hoy:** solo se envía el correo de confirmación al crear la cita (con reintentos).
- **Decisión:** ¿enviar recordatorio automático 24 h antes? ¿Por correo solamente o
  también WhatsApp?

### P5 · Promociones y cupones al agendar
- **Hoy:** el esquema de `Promocion`/`Cupon`/puntos existe en BD pero **ningún descuento
  se aplica**; toda cita se cobra a precio de lista.
- **Decisión:** ¿activamos la promo de inauguración (15%) en el flujo de reserva? ¿Los
  cupones los crea el administrador y el cliente los teclea al agendar?

### P6 · Límite de citas por cliente
- **Hoy:** un mismo cliente puede agendar todas las citas que quiera, incluso solapadas
  consigo mismo y varias el mismo día.
- **Decisión:** ¿bloqueamos citas solapadas del mismo cliente? ¿Tope por día/semana?
  ¿Se pide algún pago/anticipo para firmar la cita?

### P7 · Gestión de horarios y bloqueos desde la app
- **Hoy:** horarios de la clínica y bloqueos (vacaciones, capacitación) están en BD y se
  respetan, pero **solo se cambian por un desarrollador**; no hay pantalla.
- **Decisión:** ¿el administrador gestiona horarios/feriados/vacaciones desde la UI?
  ¿Con qué permiso?

### P8 · Español, inglés o ambos
- **Hoy:** toda la interfaz y los correos están en español.
- **Decisión:** la clínica está en Langley, BC — ¿se necesita inglés (o bilingüe) para
  clientes locales?

### P9 · Acceso a la contabilidad y reportes
- **Hoy:** los datos contables ya se generan solos, pero **no hay pantalla** para verlos;
  están pensados para reporte diario/mensual por servicio.
- **Decisión:** ¿quién los ve (solo Administrador? Recepción?) y con qué desglose
  (por servicio, por esteticista, comparativo mensual, exportar)?

### P10 · Expediente breve del cliente en cada cita
- **Hoy:** la cita guarda notas libres y contacto; no hay historial clínico ni ficha de piel.
- **Decisión:** ¿se quiere un registro de alergias/contraindicaciones visible al agendar,
  aunque sea mínimo? (Impacto en privacidad: datos de salud.)

---

## 5. Resumen: autogestión ya entregada vs decisiones abiertas

| Capacidad | Estado |
|---|---|
| Ver catálogo y precios sin cuenta | ✅ Hoy |
| Elegir su propia hora en calendario en vivo | ✅ Hoy |
| Reservar sin registrarse | ✅ Hoy |
| Hora protegida mientras completa datos (reserva suave) | ✅ Hoy |
| Sugerencias automáticas de horario alternativo | ✅ Hoy |
| Confirmación por correo con reintentos | ✅ Hoy |
| Ver sus citas y su estado | ✅ Hoy |
| Contabilidad diaria/mensual automática (staff) | ✅ Datos hoy · pantalla pendiente (P9) |
| Cancelar / reagendar por sí mismo | ❌ Decisión P2 |
| Elegir si su cita requiere aceptación | ❌ Decisión P1 |
| Recordatorio antes de la visita | ❌ Decisión P4 |
| Aplicar promociones al agendar | ❌ Decisión P5 |
