import { ImageResponse } from 'next/og'

// Route segment config
export const runtime = 'edge'

// Image metadata
export const alt = 'Mangalith - Lector de Manga de Código Abierto'
export const size = {
  width: 1200,
  height: 630,
}
export const contentType = 'image/png'

// Image generation
export default async function Image() {
  return new ImageResponse(
    (
      <div
        style={{
          background: 'linear-gradient(135deg, #000000 0%, #1a1a1a 50%, #333333 100%)',
          width: '100%',
          height: '100%',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          flexDirection: 'column',
          fontFamily: 'system-ui, sans-serif',
        }}
      >
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            marginBottom: '40px',
          }}
        >
          <div
            style={{
              width: '80px',
              height: '80px',
              background: 'white',
              borderRadius: '16px',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              fontSize: '48px',
              fontWeight: 'bold',
              color: 'black',
              marginRight: '24px',
            }}
          >
            M
          </div>
          <div
            style={{
              fontSize: '72px',
              fontWeight: 'bold',
              color: 'white',
            }}
          >
            Mangalith
          </div>
        </div>
        
        <div
          style={{
            fontSize: '32px',
            color: '#cccccc',
            textAlign: 'center',
            maxWidth: '800px',
            lineHeight: 1.4,
          }}
        >
          Tu biblioteca personal de manga digital
        </div>
        
        <div
          style={{
            fontSize: '24px',
            color: '#999999',
            marginTop: '24px',
            textAlign: 'center',
          }}
        >
          Organiza • Lee • Administra • Código Abierto
        </div>
      </div>
    ),
    {
      ...size,
    }
  )
}