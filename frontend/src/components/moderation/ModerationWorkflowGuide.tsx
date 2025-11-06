'use client'

import { useState } from 'react'
import { Card } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Alert } from '@/components/ui/alert'
import { 
  BookOpen, 
  CheckCircle, 
  XCircle, 
  AlertTriangle,
  Shield,
  Eye,
  MessageSquare,
  Flag,
  Users,
  Clock,
  Target,
  HelpCircle,
  ChevronRight,
  ChevronDown,
  Lightbulb,
  AlertCircle,
  Info
} from 'lucide-react'

interface WorkflowStep {
  id: string
  title: string
  description: string
  icon: React.ComponentType<{ className?: string }>
  actions: string[]
  tips: string[]
  warnings?: string[]
}

interface PolicySection {
  id: string
  title: string
  description: string
  rules: PolicyRule[]
  examples: PolicyExample[]
}

interface PolicyRule {
  id: string
  rule: string
  severity: 'high' | 'medium' | 'low'
  action: 'reject' | 'request-revision' | 'approve-with-warning'
}

interface PolicyExample {
  scenario: string
  decision: string
  reasoning: string
}

interface ModerationWorkflowGuideProps {
  onClose?: () => void
  compact?: boolean
}

export function ModerationWorkflowGuide({ onClose, compact = false }: ModerationWorkflowGuideProps) {
  const [activeStep, setActiveStep] = useState<string | null>(null)
  const [activePolicy, setActivePolicy] = useState<string | null>(null)

  const workflowSteps: WorkflowStep[] = [
    {
      id: 'initial-review',
      title: 'Revisión Inicial',
      description: 'Evaluación rápida del contenido y metadatos',
      icon: Eye,
      actions: [
        'Verificar que el archivo se haya procesado correctamente',
        'Revisar metadatos básicos (título, descripción, etiquetas)',
        'Comprobar si hay reportes comunitarios previos',
        'Evaluar la calidad general del contenido'
      ],
      tips: [
        'Dedica 2-3 minutos a esta revisión inicial',
        'Si hay múltiples reportes, prioriza la revisión',
        'Verifica que las etiquetas sean apropiadas y precisas'
      ],
      warnings: [
        'No apruebes contenido sin revisar todas las páginas',
        'Presta atención especial a contenido marcado como NSFW'
      ]
    },
    {
      id: 'content-evaluation',
      title: 'Evaluación de Contenido',
      description: 'Revisión detallada del contenido y calidad',
      icon: BookOpen,
      actions: [
        'Revisar todas las páginas del manga',
        'Verificar la calidad de las imágenes',
        'Comprobar que el contenido esté completo',
        'Evaluar si cumple con las políticas de la plataforma'
      ],
      tips: [
        'Usa las herramientas de zoom para verificar la calidad',
        'Asegúrate de que el orden de las páginas sea correcto',
        'Verifica que no falten páginas importantes'
      ],
      warnings: [
        'Rechaza contenido con calidad muy baja o ilegible',
        'No apruebes contenido incompleto o corrupto'
      ]
    },
    {
      id: 'policy-check',
      title: 'Verificación de Políticas',
      description: 'Asegurar cumplimiento con las reglas de la plataforma',
      icon: Shield,
      actions: [
        'Verificar clasificación de edad apropiada',
        'Comprobar cumplimiento de políticas de contenido',
        'Evaluar si requiere etiquetas NSFW',
        'Verificar derechos de autor y licencias'
      ],
      tips: [
        'Consulta las guías de clasificación cuando tengas dudas',
        'Es mejor ser conservador con las clasificaciones de edad',
        'Documenta cualquier decisión controvertida'
      ],
      warnings: [
        'Nunca apruebes contenido que viole derechos de autor',
        'Sé estricto con contenido que pueda ser inapropiado para menores'
      ]
    },
    {
      id: 'decision-making',
      title: 'Toma de Decisión',
      description: 'Decidir la acción apropiada y proporcionar retroalimentación',
      icon: Target,
      actions: [
        'Seleccionar la acción apropiada (aprobar/rechazar/revisar)',
        'Escribir comentarios claros y constructivos',
        'Establecer clasificación de contenido si se aprueba',
        'Documentar la razón de la decisión'
      ],
      tips: [
        'Sé específico en tus comentarios de retroalimentación',
        'Proporciona sugerencias constructivas para mejoras',
        'Mantén un tono profesional y respetuoso'
      ],
      warnings: [
        'No tomes decisiones apresuradas en casos complejos',
        'Consulta con otros moderadores si no estás seguro'
      ]
    }
  ]

  const policyGuides: PolicySection[] = [
    {
      id: 'content-rating',
      title: 'Clasificación de Contenido',
      description: 'Guías para asignar clasificaciones de edad apropiadas',
      rules: [
        {
          id: 'general',
          rule: 'General: Apropiado para todas las edades, sin violencia gráfica ni contenido sexual',
          severity: 'low',
          action: 'approve-with-warning'
        },
        {
          id: 'teen',
          rule: 'Adolescente: Violencia leve, temas maduros, sin contenido sexual explícito',
          severity: 'medium',
          action: 'approve-with-warning'
        },
        {
          id: 'mature',
          rule: 'Maduro: Violencia moderada, temas adultos, contenido sexual sugerido',
          severity: 'medium',
          action: 'request-revision'
        },
        {
          id: 'adult',
          rule: 'Adulto: Contenido explícito, violencia gráfica, contenido sexual explícito',
          severity: 'high',
          action: 'reject'
        }
      ],
      examples: [
        {
          scenario: 'Manga de acción con peleas pero sin sangre excesiva',
          decision: 'Clasificar como Adolescente',
          reasoning: 'Violencia presente pero no gráfica, apropiado para 13+'
        },
        {
          scenario: 'Romance con escenas sugerentes pero sin desnudez',
          decision: 'Clasificar como Maduro',
          reasoning: 'Contenido romántico maduro, apropiado para 17+'
        }
      ]
    },
    {
      id: 'quality-standards',
      title: 'Estándares de Calidad',
      description: 'Criterios para evaluar la calidad técnica del contenido',
      rules: [
        {
          id: 'resolution',
          rule: 'Resolución mínima: 800px en el lado más largo',
          severity: 'medium',
          action: 'request-revision'
        },
        {
          id: 'compression',
          rule: 'Compresión excesiva que afecte la legibilidad',
          severity: 'medium',
          action: 'request-revision'
        },
        {
          id: 'completeness',
          rule: 'Capítulos incompletos o páginas faltantes',
          severity: 'high',
          action: 'reject'
        },
        {
          id: 'corruption',
          rule: 'Archivos corruptos o páginas no legibles',
          severity: 'high',
          action: 'reject'
        }
      ],
      examples: [
        {
          scenario: 'Imágenes borrosas pero legibles',
          decision: 'Solicitar revisión',
          reasoning: 'Calidad subóptima pero aceptable con mejoras'
        },
        {
          scenario: 'Páginas completamente ilegibles',
          decision: 'Rechazar',
          reasoning: 'Calidad inaceptable que impide la lectura'
        }
      ]
    },
    {
      id: 'copyright-policy',
      title: 'Políticas de Derechos de Autor',
      description: 'Guías para manejar contenido con posibles problemas de copyright',
      rules: [
        {
          id: 'official-content',
          rule: 'Contenido oficial sin autorización explícita',
          severity: 'high',
          action: 'reject'
        },
        {
          id: 'fan-content',
          rule: 'Contenido de fans claramente etiquetado como tal',
          severity: 'low',
          action: 'approve-with-warning'
        },
        {
          id: 'original-content',
          rule: 'Contenido original del creador',
          severity: 'low',
          action: 'approve-with-warning'
        }
      ],
      examples: [
        {
          scenario: 'Manga oficial escaneado sin permiso',
          decision: 'Rechazar inmediatamente',
          reasoning: 'Violación clara de derechos de autor'
        },
        {
          scenario: 'Doujinshi (contenido de fans) claramente etiquetado',
          decision: 'Aprobar con etiquetas apropiadas',
          reasoning: 'Contenido de fans permitido con etiquetado correcto'
        }
      ]
    }
  ]

  const quickActions = [
    {
      title: 'Aprobar Rápidamente',
      description: 'Para contenido que claramente cumple todos los estándares',
      icon: CheckCircle,
      color: 'text-green-600',
      bgColor: 'bg-green-50 border-green-200'
    },
    {
      title: 'Rechazar por Calidad',
      description: 'Para contenido con problemas técnicos graves',
      icon: XCircle,
      color: 'text-red-600',
      bgColor: 'bg-red-50 border-red-200'
    },
    {
      title: 'Solicitar Revisión',
      description: 'Para contenido que necesita mejoras menores',
      icon: AlertTriangle,
      color: 'text-yellow-600',
      bgColor: 'bg-yellow-50 border-yellow-200'
    },
    {
      title: 'Escalar a Supervisor',
      description: 'Para casos complejos o controvertidos',
      icon: Flag,
      color: 'text-purple-600',
      bgColor: 'bg-purple-50 border-purple-200'
    }
  ]

  if (compact) {
    return (
      <Card className="p-4">
        <div className="flex items-center justify-between mb-3">
          <h3 className="font-semibold flex items-center gap-2">
            <HelpCircle className="w-4 h-4" />
            Guía Rápida
          </h3>
          {onClose && (
            <Button variant="ghost" size="sm" onClick={onClose}>
              ×
            </Button>
          )}
        </div>
        <div className="space-y-2">
          {quickActions.map((action, index) => (
            <div key={index} className={`p-2 rounded border ${action.bgColor}`}>
              <div className="flex items-center gap-2">
                <action.icon className={`w-4 h-4 ${action.color}`} />
                <div>
                  <p className="text-sm font-medium">{action.title}</p>
                  <p className="text-xs text-muted-foreground">{action.description}</p>
                </div>
              </div>
            </div>
          ))}
        </div>
      </Card>
    )
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold">Guía de Moderación</h2>
          <p className="text-muted-foreground">
            Flujo de trabajo y políticas para la moderación de contenido
          </p>
        </div>
        {onClose && (
          <Button variant="outline" onClick={onClose}>
            Cerrar Guía
          </Button>
        )}
      </div>

      <Tabs defaultValue="workflow" className="space-y-4">
        <TabsList className="grid w-full grid-cols-3">
          <TabsTrigger value="workflow">
            <Target className="w-4 h-4 mr-2" />
            Flujo de Trabajo
          </TabsTrigger>
          <TabsTrigger value="policies">
            <Shield className="w-4 h-4 mr-2" />
            Políticas
          </TabsTrigger>
          <TabsTrigger value="quick-reference">
            <Lightbulb className="w-4 h-4 mr-2" />
            Referencia Rápida
          </TabsTrigger>
        </TabsList>

        <TabsContent value="workflow" className="space-y-4">
          <div className="space-y-4">
            {workflowSteps.map((step, index) => {
              const Icon = step.icon
              const isActive = activeStep === step.id
              
              return (
                <Card key={step.id} className="overflow-hidden">
                  <button
                    onClick={() => setActiveStep(isActive ? null : step.id)}
                    className="w-full p-4 text-left hover:bg-accent transition-colors"
                  >
                    <div className="flex items-center gap-4">
                      <div className="flex items-center gap-3">
                        <div className="w-8 h-8 bg-primary/10 rounded-full flex items-center justify-center">
                          <span className="text-sm font-bold text-primary">{index + 1}</span>
                        </div>
                        <Icon className="w-5 h-5 text-primary" />
                      </div>
                      <div className="flex-1">
                        <h3 className="font-semibold">{step.title}</h3>
                        <p className="text-sm text-muted-foreground">{step.description}</p>
                      </div>
                      {isActive ? (
                        <ChevronDown className="w-4 h-4" />
                      ) : (
                        <ChevronRight className="w-4 h-4" />
                      )}
                    </div>
                  </button>
                  
                  {isActive && (
                    <div className="px-4 pb-4 space-y-4 border-t bg-muted/30">
                      <div>
                        <h4 className="font-medium mb-2 flex items-center gap-2">
                          <CheckCircle className="w-4 h-4 text-green-600" />
                          Acciones a Realizar
                        </h4>
                        <ul className="space-y-1">
                          {step.actions.map((action, actionIndex) => (
                            <li key={actionIndex} className="text-sm flex items-start gap-2">
                              <span className="text-muted-foreground">•</span>
                              {action}
                            </li>
                          ))}
                        </ul>
                      </div>

                      <div>
                        <h4 className="font-medium mb-2 flex items-center gap-2">
                          <Lightbulb className="w-4 h-4 text-yellow-600" />
                          Consejos Útiles
                        </h4>
                        <ul className="space-y-1">
                          {step.tips.map((tip, tipIndex) => (
                            <li key={tipIndex} className="text-sm flex items-start gap-2">
                              <span className="text-muted-foreground">💡</span>
                              {tip}
                            </li>
                          ))}
                        </ul>
                      </div>

                      {step.warnings && (
                        <div>
                          <h4 className="font-medium mb-2 flex items-center gap-2">
                            <AlertCircle className="w-4 h-4 text-red-600" />
                            Advertencias Importantes
                          </h4>
                          <ul className="space-y-1">
                            {step.warnings.map((warning, warningIndex) => (
                              <li key={warningIndex} className="text-sm flex items-start gap-2">
                                <span className="text-red-500">⚠️</span>
                                {warning}
                              </li>
                            ))}
                          </ul>
                        </div>
                      )}
                    </div>
                  )}
                </Card>
              )
            })}
          </div>
        </TabsContent>

        <TabsContent value="policies" className="space-y-4">
          <div className="space-y-4">
            {policyGuides.map((policy) => {
              const isActive = activePolicy === policy.id
              
              return (
                <Card key={policy.id} className="overflow-hidden">
                  <button
                    onClick={() => setActivePolicy(isActive ? null : policy.id)}
                    className="w-full p-4 text-left hover:bg-accent transition-colors"
                  >
                    <div className="flex items-center justify-between">
                      <div>
                        <h3 className="font-semibold">{policy.title}</h3>
                        <p className="text-sm text-muted-foreground">{policy.description}</p>
                      </div>
                      {isActive ? (
                        <ChevronDown className="w-4 h-4" />
                      ) : (
                        <ChevronRight className="w-4 h-4" />
                      )}
                    </div>
                  </button>
                  
                  {isActive && (
                    <div className="px-4 pb-4 space-y-4 border-t bg-muted/30">
                      <div>
                        <h4 className="font-medium mb-3">Reglas y Criterios</h4>
                        <div className="space-y-2">
                          {policy.rules.map((rule) => (
                            <div key={rule.id} className="p-3 border rounded-lg">
                              <div className="flex items-start justify-between gap-3">
                                <p className="text-sm flex-1">{rule.rule}</p>
                                <div className="flex items-center gap-2">
                                  <Badge 
                                    className={
                                      rule.severity === 'high' 
                                        ? 'bg-red-100 text-red-800'
                                        : rule.severity === 'medium'
                                        ? 'bg-yellow-100 text-yellow-800'
                                        : 'bg-green-100 text-green-800'
                                    }
                                  >
                                    {rule.severity === 'high' ? 'Alta' : rule.severity === 'medium' ? 'Media' : 'Baja'}
                                  </Badge>
                                </div>
                              </div>
                            </div>
                          ))}
                        </div>
                      </div>

                      <div>
                        <h4 className="font-medium mb-3">Ejemplos Prácticos</h4>
                        <div className="space-y-3">
                          {policy.examples.map((example, index) => (
                            <div key={index} className="p-3 bg-blue-50 border border-blue-200 rounded-lg">
                              <p className="font-medium text-sm mb-1">{example.scenario}</p>
                              <p className="text-sm text-blue-700 mb-1">
                                <strong>Decisión:</strong> {example.decision}
                              </p>
                              <p className="text-xs text-blue-600">
                                <strong>Razonamiento:</strong> {example.reasoning}
                              </p>
                            </div>
                          ))}
                        </div>
                      </div>
                    </div>
                  )}
                </Card>
              )
            })}
          </div>
        </TabsContent>

        <TabsContent value="quick-reference" className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {quickActions.map((action, index) => {
              const Icon = action.icon
              return (
                <Card key={index} className={`p-4 ${action.bgColor}`}>
                  <div className="flex items-start gap-3">
                    <Icon className={`w-6 h-6 ${action.color} mt-1`} />
                    <div>
                      <h3 className="font-semibold mb-1">{action.title}</h3>
                      <p className="text-sm text-muted-foreground">{action.description}</p>
                    </div>
                  </div>
                </Card>
              )
            })}
          </div>

          <Card className="p-4">
            <h3 className="font-semibold mb-3 flex items-center gap-2">
              <Clock className="w-4 h-4" />
              Tiempos de Respuesta Objetivo
            </h3>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="text-center p-3 bg-green-50 rounded-lg">
                <div className="text-lg font-bold text-green-600">&lt; 2h</div>
                <div className="text-sm text-green-700">Contenido estándar</div>
              </div>
              <div className="text-center p-3 bg-yellow-50 rounded-lg">
                <div className="text-lg font-bold text-yellow-600">&lt; 4h</div>
                <div className="text-sm text-yellow-700">Contenido reportado</div>
              </div>
              <div className="text-center p-3 bg-red-50 rounded-lg">
                <div className="text-lg font-bold text-red-600">&lt; 1h</div>
                <div className="text-sm text-red-700">Contenido urgente</div>
              </div>
            </div>
          </Card>

          <Alert>
            <Info className="w-4 h-4" />
            <div>
              <p className="font-medium">Recordatorio Importante</p>
              <p className="text-sm text-muted-foreground mt-1">
                Cuando tengas dudas sobre una decisión, siempre es mejor consultar con un supervisor 
                o moderador senior antes de tomar una acción irreversible.
              </p>
            </div>
          </Alert>
        </TabsContent>
      </Tabs>
    </div>
  )
}