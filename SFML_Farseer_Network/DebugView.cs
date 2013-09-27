using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.Window;
using Microsoft.Xna.Framework;
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

    public class DebugView
    {
        private Game _game;
        private PhysicsManager _physicsManager;
        private VertexArray _triangleVertices;
        private VertexArray _lineVertices;
        private Color _staticColor;
        private Color _dynamicColor;

        public DebugView(Game game, PhysicsManager physicsManager)
        {
            _game = game;
            _physicsManager = physicsManager;
            _triangleVertices = new VertexArray(PrimitiveType.Triangles);
            _lineVertices = new VertexArray(PrimitiveType.Lines);
            _staticColor = new Color(50, 150, 50);
            _dynamicColor = new Color(50, 50, 150);
        }

        public void DrawCircle(Vector2 center, float radius, ref Color color)
        {
            CircleShape circle = new CircleShape(radius);

            circle.Position = new Vector2f(center.X, center.Y);
            _game.window.Draw(circle);
        }

        public void DrawPolygon(Vector2[] vertices, int count, ref Color color, bool closed = true)
        {
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

        public void DrawSegment(Vector2 start, Vector2 end, ref Color color)
        {
            _lineVertices.Append(new Vertex(new Vector2f(start.X, start.Y), color));
            _lineVertices.Append(new Vertex(new Vector2f(end.X, end.Y), color));
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

                        if (body.BodyType == BodyType.Static)
                        {
                            DrawCircle(position, circleShape.Radius, ref _staticColor);
                        }
                        else
                        {
                            DrawCircle(position, circleShape.Radius, ref _dynamicColor);
                        }
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

                        if (body.BodyType == BodyType.Static)
                        {
                            DrawPolygon(vertices, count, ref _staticColor);
                        }
                        else
                        {
                            DrawPolygon(vertices, count, ref _dynamicColor);
                        }
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
