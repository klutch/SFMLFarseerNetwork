using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.Window;
using Microsoft.Xna.Framework;
using FarseerPhysics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using SFML_Farseer_Network.Managers;

namespace SFML_Farseer_Network
{
    using FCircleShape = FarseerPhysics.Collision.Shapes.CircleShape;
    using FPolygonShape = FarseerPhysics.Collision.Shapes.PolygonShape;
    using FShape = FarseerPhysics.Collision.Shapes.Shape;
    using ShapeType = FarseerPhysics.Collision.Shapes.ShapeType;
    using FTransform = FarseerPhysics.Common.Transform;

    public class DebugView : DebugViewBase
    {
        private Game _game;
        private PhysicsManager _physicsManager;
        private VertexArray _triangleVertices;
        private VertexArray _lineVertices;

        public DebugView(Game game, PhysicsManager physicsManager) : base(physicsManager.world)
        {
            _game = game;
            _physicsManager = physicsManager;
            _triangleVertices = new VertexArray(PrimitiveType.Triangles);
            _lineVertices = new VertexArray(PrimitiveType.Lines);
        }

        private void createColor(float red, float green, float blue, out Color color)
        {
            color = new Color((byte)(255 * red), (byte)(255 * green), (byte)(255 * blue), (byte)255);
        }

        public override void DrawCircle(Vector2 center, float radius, float red, float blue, float green)
        {
            Color color;
            CircleShape circle = new CircleShape(radius);

            createColor(red, green, blue, out color);
            circle.Position = new Vector2f(center.X, center.Y);
            _game.window.Draw(circle);
        }

        public override void DrawPolygon(Vector2[] vertices, int count, float red, float blue, float green, bool closed = true)
        {
            Color color;

            createColor(red, green, blue, out color);
            if (count == 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    _triangleVertices.Append(new Vertex(new Vector2f(vertices[i].X, vertices[i].Y), color));
                }
            }
            else if (count == 4)
            {
                _triangleVertices.Append(new Vertex(new Vector2f(vertices[0].X, vertices[0].Y), color));
                _triangleVertices.Append(new Vertex(new Vector2f(vertices[1].X, vertices[1].Y), color));
                _triangleVertices.Append(new Vertex(new Vector2f(vertices[2].X, vertices[2].Y), color));
                _triangleVertices.Append(new Vertex(new Vector2f(vertices[0].X, vertices[0].Y), color));
                _triangleVertices.Append(new Vertex(new Vector2f(vertices[2].X, vertices[2].Y), color));
                _triangleVertices.Append(new Vertex(new Vector2f(vertices[3].X, vertices[3].Y), color));
            }
        }

        public override void DrawSegment(Vector2 start, Vector2 end, float red, float blue, float green)
        {
            Color color;

            createColor(red, green, blue, out color);
            _lineVertices.Append(new Vertex(new Vector2f(start.X, start.Y), color));
            _lineVertices.Append(new Vertex(new Vector2f(end.X, end.Y), color));
        }

        public override void DrawSolidCircle(Vector2 center, float radius, Vector2 axis, float red, float blue, float green)
        {
            Color color;

            createColor(red, green, blue, out color);
            DrawCircle(center, radius, red, blue, green);
        }

        public override void DrawSolidPolygon(Vector2[] vertices, int count, float red, float blue, float green)
        {
            DrawPolygon(vertices, count, red, blue, green);
        }

        public override void DrawTransform(ref FTransform transform)
        {
            throw new NotImplementedException();
        }

        public void draw()
        {
            foreach (Body body in _game.physicsManager.world.BodyList)
            {
                foreach (Fixture fixture in body.FixtureList)
                {
                    FShape shape = fixture.Shape;

                    if (shape.ShapeType == ShapeType.Circle)
                    {
                        FCircleShape circleShape = shape as FCircleShape;
                        Vector2 position = body.GetWorldVector(circleShape.Position);

                        DrawCircle(position, circleShape.Radius, 1f, 1f, 1f);
                    }
                    else if (shape.ShapeType == ShapeType.Polygon)
                    {
                        FPolygonShape polygonShape = shape as FPolygonShape;
                        int count = polygonShape.Vertices.Count;
                        Vector2[] vertices = new Vector2[count];
                        FTransform xf;

                        body.GetTransform(out xf);
                        for (int i = 0; i < count; i++)
                        {
                            vertices[i] = MathUtils.Mul(ref xf, polygonShape.Vertices[i]);
                        }

                        DrawPolygon(vertices, count, 1f, 1f, 1f);
                    }
                }
            }

            _game.window.Draw(_lineVertices);
            _game.window.Draw(_triangleVertices);
        }

        public void reset()
        {
            _triangleVertices.Clear();
            _lineVertices.Clear();
        }
    }
}
