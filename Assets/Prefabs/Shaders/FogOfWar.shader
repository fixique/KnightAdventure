Shader "Custom/FogOfWar"
{
    Properties
    {
        // SpriteRenderer автоматически передаёт сюда текстуру спрайта.
        [PerRendererData]
        _MainTex("Sprite Texture", 2D) = "white" {}

        // Постоянная маска исследованной территории:
        // чёрный — не исследовано;
        // белый — исследовано.
        _ExploredTex("Explored Texture", 2D) = "black" {}

        _FogColor("Fog Color", Color) = (0, 0, 0, 1)

        _UnexploredAlpha("Unexplored Alpha", Range(0, 1)) = 1
        _ExploredAlpha("Explored Alpha", Range(0, 1)) = 0.55

        _VisionRadius("Vision Radius", Float) = 5
        _VisionSoftness("Vision Softness", Float) = 1

        // Эти параметры устанавливаются из FogOfWarController.
        _PlayerPosition("Player Position", Vector) = (0, 0, 0, 0)
        _WorldMin("World Min", Vector) = (0, 0, 0, 0)
        _WorldSize("World Size", Vector) = (1, 1, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "CanUseSpriteAtlas" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha

        Cull Off
        ZWrite Off
        ZTest LEqual

        Pass
        {
            Name "FogOfWar"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;

                // Мировая позиция пикселя FogOfWar.
                float3 positionWS : TEXCOORD0;

                // UV базового спрайта Square.
                float2 uv : TEXCOORD1;

                // Цвет SpriteRenderer.
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_ExploredTex);
            SAMPLER(sampler_ExploredTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _FogColor;

                float _UnexploredAlpha;
                float _ExploredAlpha;

                float _VisionRadius;
                float _VisionSoftness;

                float4 _PlayerPosition;
                float4 _WorldMin;
                float4 _WorldSize;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs positionInputs =
                    GetVertexPositionInputs(input.positionOS.xyz);

                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;

                output.uv = input.uv;
                output.color = input.color;

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // Получаем базовую текстуру SpriteRenderer.
                half4 spriteColor = SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    input.uv
                );

                float2 worldPosition = input.positionWS.xy;

                /*
                 * Переводим мировую позицию пикселя в координаты
                 * исследованной текстуры от 0 до 1.
                 */
                float2 exploredUV =
                    (worldPosition - _WorldMin.xy) /
                    max(_WorldSize.xy, float2(0.0001, 0.0001));

                bool outsideMap =
                    exploredUV.x < 0.0 ||
                    exploredUV.x > 1.0 ||
                    exploredUV.y < 0.0 ||
                    exploredUV.y > 1.0;

                /*
                 * Вне карты не читаем текстуру по некорректным UV.
                 * Территория считается неисследованной.
                 */
                float explored = 0.0;

                if (!outsideMap)
                {
                    explored = SAMPLE_TEXTURE2D(
                        _ExploredTex,
                        sampler_ExploredTex,
                        exploredUV
                    ).r;
                }

                /*
                 * Выбираем прозрачность тумана:
                 *
                 * explored = 0 → UnexploredAlpha;
                 * explored = 1 → ExploredAlpha.
                 */
                float fogAlpha = lerp(
                    _UnexploredAlpha,
                    _ExploredAlpha,
                    explored
                );

                float distanceToPlayer = distance(
                    worldPosition,
                    _PlayerPosition.xy
                );

                /*
                 * Защита от ситуации, когда softness больше radius.
                 */
                float softness = min(
                    max(_VisionSoftness, 0.0001),
                    _VisionRadius
                );

                /*
                 * currentlyVisible:
                 *
                 * 1 — рядом с игроком;
                 * 0 — за пределами радиуса.
                 *
                 * Между ними создаётся плавный край.
                 */
                float currentlyVisible =
                    1.0 - smoothstep(
                        _VisionRadius - softness,
                        _VisionRadius,
                        distanceToPlayer
                    );

                // В текущей видимой области туман исчезает.
                fogAlpha *= 1.0 - currentlyVisible;

                if (outsideMap)
                {
                    fogAlpha = _UnexploredAlpha;
                }

                /*
                 * Учитываем:
                 * - цвет материала;
                 * - Color у SpriteRenderer;
                 * - альфу самого Square sprite.
                 */
                half3 finalColor =
                    _FogColor.rgb *
                    input.color.rgb *
                    spriteColor.rgb;

                half finalAlpha =
                    fogAlpha *
                    _FogColor.a *
                    input.color.a *
                    spriteColor.a;

                return half4(finalColor, finalAlpha);
            }

            ENDHLSL
        }
    }

    Fallback Off
}