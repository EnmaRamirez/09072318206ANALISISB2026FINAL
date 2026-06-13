# Historias de usuario

1. Como operador de red, quiero registrar incidentes desde la API para capturar cada evento con su severidad y tipo.
   - Criterios de aceptación: el sistema crea un incidente con estado Registrado y fecha de creación.

2. Como analista, quiero consultar incidentes activos para priorizar tareas y revisar el estado de cada caso.
   - Criterios de aceptación: la API devuelve incidentes ordenados por fecha de creación.

3. Como técnico, quiero asignarme un incidente para cerrar la brecha entre la notificación y la atención.
   - Criterios de aceptación: solo se puede asignar a un técnico válido y con especialidad compatible.

4. Como supervisor, quiero evitar sobrecarga de trabajo para cada técnico, limitando a 3 incidentes activos simultáneos.
   - Criterios de aceptación: al superar el límite, la API devuelve error de negocio.

5. Como técnico, quiero actualizar el estado del incidente a En Progreso, Resuelto o Cerrado para reflejar el avance real.
   - Criterios de aceptación: los cambios respetan la secuencia válida del flujo.

6. Como supervisor, quiero reasignar incidentes en cualquier momento para mover la carga entre técnicos.
   - Criterios de aceptación: la API cambia el técnico asignado y registra la acción.

7. Como analista, quiero liberar un incidente para que otro técnico pueda tomarlo cuando el anterior no lo atiende.
   - Criterios de aceptación: el incidente vuelve a estado Registrado y queda sin técnico asignado.

8. Como responsable de SLA, quiero que los incidentes críticos o urgentes se escalen si no se atienden en 2 horas.
   - Criterios de aceptación: la API marca el incidente como Escalado automáticamente.

9. Como administrador, quiero consultar el historial de estado para auditar el seguimiento de cada incidente.
   - Criterios de aceptación: la API devuelve el log de cambios por incidente.

10. Como gestor, quiero consultar reportes de incidentes para identificar cuellos de botella y cumplimiento de SLA.
   - Criterios de aceptación: la API expone un resumen por estado, severidad y incidentes vencidos.

11. Como técnico de fibra, quiero recibir solo incidentes que correspondan a mi especialidad para evitar errores de asignación.
   - Criterios de aceptación: la API rechaza asignaciones sin especialidad compatible.

12. Como analista de operaciones, quiero ver el tiempo máximo de resolución según la severidad para planificar recursos.
   - Criterios de aceptación: el sistema calcula una fecha límite de resolución dinámica al crear el incidente.
